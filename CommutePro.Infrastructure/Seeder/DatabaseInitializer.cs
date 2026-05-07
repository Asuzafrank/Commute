using CommutePro.Domain.Entities;
using CommutePro.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Seeder
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userSeederLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserSeeder>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
            var gtfsImporter = scope.ServiceProvider.GetRequiredService<GtfsStaticImporter>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                // 1. Apply pending migrations
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully");

                // 2. Seed users
                logger.LogInformation("Seeding users...");
                var userSeeder = new UserSeeder(userManager, userSeederLogger);
                await userSeeder.SeedUsersAsync();
                logger.LogInformation("Users seeded successfully");

                // 3. Seed GTFS static data (only if empty)
                var hasStops = await context.Stops.AnyAsync();
                if (!hasStops)
                {
                    logger.LogInformation("No GTFS data found, importing static data...");

                    var gtfsFolder = configuration["Gtfs:StaticFolderPath"];
                    if (string.IsNullOrEmpty(gtfsFolder))
                    {
                        logger.LogWarning("GTFS static folder path not configured. Skipping GTFS import.");
                    }
                    else if (!Directory.Exists(gtfsFolder))
                    {
                        logger.LogWarning("GTFS static folder not found: {Folder}. Skipping GTFS import.", gtfsFolder);
                    }
                    else
                    {
                        var result = await gtfsImporter.ImportFromFilesAsync(gtfsFolder);
                        if (result.Success)
                        {
                            logger.LogInformation("GTFS static data imported: {Stops} stops, {Routes} routes, {Trips} trips, {StopTimes} stop times",
                                result.StopsImported, result.RoutesImported, result.TripsImported, result.StopTimesImported);
                        }
                        else
                        {
                            logger.LogError("GTFS static import failed: {Message}", result.Message);
                        }
                    }
                }
                else
                {
                    logger.LogInformation("GTFS data already exists, skipping import");
                }

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }
    }
}
