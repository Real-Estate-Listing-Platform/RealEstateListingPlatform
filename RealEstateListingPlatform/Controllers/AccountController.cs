using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AccountController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model.FullName, model.Email, model.Password, model.PhoneNumber);

            if (result.Success)
            {
                return RedirectToAction("VerifyEmail", new { email = model.Email });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.VerifyOtpAsync(model.Email, model.OtpCode);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required to resend OTP.";
                return RedirectToAction("Register");
            }

            var result = await _authService.ResendOtpAsync(email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("VerifyEmail", new { email = email });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.ForgotPasswordAsync(model.Email);

            if (result.Success)
            {
                return RedirectToAction("VerifyResetOtp", new { email = model.Email });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyResetOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.VerifyResetOtpAsync(model.Email, model.OtpCode);

            if (result.Success)
            {
                return RedirectToAction("ResetPassword", new { email = model.Email, token = result.Token });
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return RedirectToAction("ForgotPassword");
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendForgotPasswordOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required to resend OTP.";
                return RedirectToAction("ForgotPassword");
            }

            var result = await _authService.ForgotPasswordAsync(email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("VerifyResetOtp", new { email = email });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (result.Success)
            {
                var user = await _userService.GetUserByEmail(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Login failed: user not found.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
