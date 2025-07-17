using levelme.Data;
using levelme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace levelme.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Accounts.AnyAsync(a => a.Username == request.Username))
                return BadRequest("Такое имя пользователя уже зарегестрированно!");

            if (await _context.Accounts.AnyAsync(a => a.Email == request.Email))
                return BadRequest("Адрес электронной почты уже зарегестрирован!");

            var account = new Accounts
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Успешная регистрация" });
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}
