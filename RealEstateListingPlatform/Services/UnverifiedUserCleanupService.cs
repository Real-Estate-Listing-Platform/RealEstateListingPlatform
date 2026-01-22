using Microsoft.EntityFrameworkCore;
using DAL.Repositories;

namespace RealEstateListingPlatform.Services
{
    public class UnverifiedUserCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnverifiedUserCleanupService> _logger;

        public UnverifiedUserCleanupService(IServiceProvider serviceProvider, ILogger<UnverifiedUserCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Unverified User Cleanup Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        
                        // Delete users who are NOT verified AND created more than 30 minutes ago
                        var threshold = DateTime.UtcNow.AddMinutes(-30);
                        
                        // ExecuteDeleteAsync is efficient for bulk deletes (EF Core 7+)
                        var deletedCount = await userRepo.GetUsersQueryable()
                            .Where(u => !u.IsEmailVerified && u.CreatedAt < threshold)
                            .ExecuteDeleteAsync(stoppingToken);

                        if (deletedCount > 0)
                        {
                            _logger.LogInformation($"Cleaned up {deletedCount} unverified users.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing cleanup.");
                }

                // Wait for 29 minutes before next run
                await Task.Delay(TimeSpan.FromMinutes(29), stoppingToken);
            }
        }
    }
}
