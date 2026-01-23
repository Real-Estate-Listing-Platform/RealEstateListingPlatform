using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DAL.Models;
using DAL.Repositories;
using DAL.Repositories.Implementation;
using BLL.Services;
using BLL.Services.Implementation;
using RealEstateListingPlatform.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RealEstateListingPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RealEstateListingPlatformContext") ?? throw new InvalidOperationException("Connection string 'RealEstateListingPlatformContext' not found.")));

// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IListingService, ListingService>();

builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache(); // For OTP caching
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<UnverifiedUserCleanupService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

// Map API controllers with attribute routing
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
