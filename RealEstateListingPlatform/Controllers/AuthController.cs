using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateListingPlatform.Data;
using RealEstateListingPlatform.Models;
using System.Security.Cryptography;
using System.Text;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RealEstateListingPlatformContext _context;

        public AuthController(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                return BadRequest(ModelState);
            }

            // Create User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = model.FullName,
                Email = model.Email,
                Phone = model.PhoneNumber,
                Role = "Seeker", // Default role
                PasswordHash = HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate a real token ideally, but for now a simple string with info
            // In a real app, use JWT library to sign this
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Email}:{user.Id}:{DateTime.Now.Ticks}"));

            return Ok(new { token = token, fullName = user.DisplayName });
        }

        // Simple helper for demo purposes. Use ASP.NET Core Identity's PasswordHasher in production.
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
