using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model.FullName, model.Email, model.Password, model.PhoneNumber);
            
            if (result.Success)
            {
                return Ok(new { message = result.Message, email = result.Email });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(model.Email, model.OtpCode);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email)) return BadRequest(new { message = "Email is required." });

            var result = await _authService.ResendOtpAsync(model.Email);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(model.Email);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.VerifyResetOtpAsync(model.Email, model.OtpCode);

            if (result.Success)
            {
                return Ok(new { token = result.Token, message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (result.Success)
            {
                return Ok(new { token = result.Token, fullName = result.Email }); // Email prop used for DisplayName in AuthService
            }
            return Unauthorized(result.Message);
        }
    }
}