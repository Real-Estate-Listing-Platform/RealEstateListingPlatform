using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;
using System.Text;
using System.Text.Json;

namespace RealEstateListingPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001"; // Default or config
                var url = $"{baseUrl}/api/auth/register";

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Optionally login the user immediately or redirect to login
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using var doc = JsonDocument.Parse(errorContent);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            // Try to parse standard ModelState errors (e.g. { "Email": ["Error 1"] })
                            foreach (var property in doc.RootElement.EnumerateObject())
                            {
                                if (property.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var error in property.Value.EnumerateArray())
                                    {
                                        ModelState.AddModelError(property.Name, error.GetString() ?? "Unknown error");
                                    }
                                }
                                else if (property.Value.ValueKind == JsonValueKind.String)
                                {
                                     // Handle single error messages like { "message": "Error..." }
                                     ModelState.AddModelError(string.Empty, property.Value.GetString() ?? "Unknown error");
                                }
                            }
                        }
                        else 
                        {
                             ModelState.AddModelError(string.Empty, "Registration failed.");
                        }
                    }
                    catch
                    {
                        // Fallback if not JSON
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
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

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";
                var url = $"{baseUrl}/api/auth/login";

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Assuming response contains a token property, e.g., { "token": "..." }
                    // We need to parse it. Let's assume a simple structure or just the token string if it's plain text.
                    // For robustness, let's try to parse as JSON.
                    string token = "";
                    try 
                    {
                         using var doc = JsonDocument.Parse(responseString);
                         if(doc.RootElement.TryGetProperty("token", out var tokenElement))
                         {
                             token = tokenElement.GetString();
                         }
                    }
                    catch
                    {
                        // Fallback or handle as appropriate if API returns just the token
                        token = responseString; 
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Store token in Cookie or Session
                        // Using Cookie for persistence if "Remember Me" is checked, otherwise Session cookie
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = model.RememberMe ? DateTime.UtcNow.AddDays(7) : null // Session cookie if null
                        };

                        Response.Cookies.Append("JWToken", token, cookieOptions);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                         ModelState.AddModelError(string.Empty, "Invalid server response.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWToken");
            return RedirectToAction("Index", "Home");
        }
    }
}
