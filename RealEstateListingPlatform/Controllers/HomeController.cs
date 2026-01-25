using System.Diagnostics;
using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //return View();
            var properties = new List<PropertyViewModel>
            {
                new PropertyViewModel {
                    Id = 1,
                    Title = "Luxury Apartment with River View",
                    Location = "District 2, Ho Chi Minh City",
                    Price = 25000000000,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Area = 75,
                    ImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=400&q=80"
                },

                new PropertyViewModel {
                    Id = 2,
                    Title = "Modern Villa with Private Pool",
                    Location = "Thao Dien, District 2",
                    Price = 12000000000,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Area = 350,
                    ImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=400&q=80"
                },

                new PropertyViewModel {
                    Id = 3,
                    Title = "Cozy Studio near Metro",
                    Location = "Binh Thanh District",
                    Price = 850000000,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    Area = 45,
                    ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=400&q=80"
                },
                new PropertyViewModel {
                    Id = 4,
                    Title = "Penthouse Sky Garden with Infinity Pool",
                    Location = "District 7, Ho Chi Minh City",
                    Price = 45000000000, // 45 Tỷ
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Area = 450,
                    ImageUrl = "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd?auto=format&fit=crop&w=800&q=80"
                },

                new PropertyViewModel {
                    Id = 5,
                    Title = "Shophouse Vinhome Central Park",
                    Location = "Binh Thanh District, HCM",
                    Price = 18500000000, // 18.5 Tỷ
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 120,
                    ImageUrl = "https://images.unsplash.com/photo-1582407947304-fd86f028f716?auto=format&fit=crop&w=800&q=80"
                },

                new PropertyViewModel {
                    Id = 6,
                    Title = "Green Garden Villa - Eco Village",
                    Location = "Thu Duc City, Ho Chi Minh",
                    Price = 28000000000, // 28 Tỷ
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Area = 280,
                    ImageUrl = "https://res.cloudinary.com/dw4e01qx8/f_auto,q_auto/images/scgcqaofgcyewluey2xi"
                }
            };

            return View(properties);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
