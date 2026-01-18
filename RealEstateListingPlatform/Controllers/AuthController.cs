using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Simulate database saving...
                return Ok(new { message = "Registration successful" });
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            // Simulate user validation
            // For testing: Accept any login or specific credentials
            if (model.Email == "admin@example.com" && model.Password == "Password123!")
            {
                // Return a dummy JWT token
                return Ok(new { token = "dummy-jwt-token-for-testing-purposes" });
            }

            // Also allow any login for easy testing if not specific
            if (ModelState.IsValid)
            {
                 return Ok(new { token = $"token-for-{model.Email}" });
            }

            return Unauthorized("Invalid credentials");
        }
    }
}
