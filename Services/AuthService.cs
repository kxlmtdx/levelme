using levelme.Data;
using levelme.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace levelme.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(Accounts account);
        string GenerateRefreshToken();
        Task<Accounts?> GetAccountByUsername(string username);
        Task<bool> ValidateCredentials(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public AuthService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<string> GenerateJwtToken(Accounts account)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<Accounts?> GetAccountByUsername(string username)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<bool> ValidateCredentials(string username, string password)
        {
            var account = await GetAccountByUsername(username);
            if (account == null) return false;

            // return BCrypt.Net.BCrypt.Verify(password, account.PasswordHash);

            // Костыль
            return account.Password == password;
        }
    }
}