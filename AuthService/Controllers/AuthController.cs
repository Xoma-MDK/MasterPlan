using AuthService.DataStorage.Entities;
using AuthService.DataStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthDbContext context) : ControllerBase
    {
        private readonly AuthDbContext _context = context;
        private readonly string _secretKey = Environment.GetEnvironmentVariable("SecretKey") ?? throw new ArgumentNullException("SecretKey");

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Хеширование пароля и сохранение пользователя
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, existingUser.PasswordHash))
            {
                return Unauthorized();
            }
            var token = new JwtSecurityToken(
                issuer: null, // Укажите, если используете издателя
                audience: null, // Укажите, если используете аудиторию
                claims:     [
                            new(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                            new(ClaimTypes.Name, existingUser.Username)
                            ],
                expires: DateTime.Now.AddMinutes(30), // Установите время истечения
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)), SecurityAlgorithms.HmacSha256));
            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
