using levelme.Data;
using levelme.Models;
using levelme.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace levelme.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, ApplicationDbContext context, IConfiguration config)
        {
            _authService = authService;
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var isValid = await _authService.ValidateCredentials(request.Username, request.Password);
            if (!isValid)
                return Unauthorized(new { Message = "Недействительные учетные данные" });

            var account = await _authService.GetAccountByUsername(request.Username);
            if (account == null)
                return Unauthorized();

            var token = await _authService.GenerateJwtToken(account);
            var refreshToken = _authService.GenerateRefreshToken();

            account.RefreshToken = refreshToken;
            account.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthenticatedResponse
            {
                Token = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            var username = principal.Identity?.Name;
            if (username == null)
                return BadRequest("Не валидный токен");

            var account = await _authService.GetAccountByUsername(username);
            if (account == null || account.RefreshToken != request.RefreshToken ||
                account.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Не валидный токен обновления!");

            var newToken = await _authService.GenerateJwtToken(account);
            var newRefreshToken = _authService.GenerateRefreshToken();

            account.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return Ok(new AuthenticatedResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Не валидный токен");

                return principal;
            }
            catch
            {
                return null;
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length > 20)
                return BadRequest(new { Errors = new { Username = "Имя пользователя должно быть от 1 до 20 символов" } });

            if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
                return BadRequest(new { Errors = new { Email = "Некорректный email" } });

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return BadRequest(new { Errors = new { Password = "Пароль должен быть не менее 6 символов" } });

            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { Errors = new { ConfirmPassword = "Пароли не совпадают" } });

            if (await _context.Accounts.AnyAsync(a => a.Username == request.Username))
                return BadRequest(new { Errors = new { Username = "Такое имя пользователя уже зарегестрированно!" } });

            if (await _context.Accounts.AnyAsync(a => a.Email == request.Email))
                return BadRequest(new { Errors = new { Email = "Адрес электронной почты уже зарегестрирован!" } });

            var account = new Accounts
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password // ХЭШ!!!
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Успешная регистрация" });
        }
    }
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AuthenticatedResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}