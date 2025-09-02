using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Seeders;
using WasteFree.Infrastructure.Services;

namespace WasteFree.App.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host,  
        int retry = 0) where TContext : ApplicationDataContext
    {
        int retryForAvailability = retry;

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();
            
            try
            {
                if (context is not null)
                {
                    context.Database.EnsureDeleted();
                    
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                    
                    context.Database.Migrate();
                    
                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);

                    // Run seeders
                    var userSeeder = services.GetService<UserSeeder>();
                    if (userSeeder != null)
                    {
                        userSeeder.SeedAsync().GetAwaiter().GetResult();
                        logger.LogInformation("Executed UserSeeder.SeedAsync");
                    }

                    var notificationTemplateSeeder = services.GetService<NotificationTemplateSeeder>();
                    if (notificationTemplateSeeder != null)
                    {
                        notificationTemplateSeeder.SeedAsync().GetAwaiter().GetResult();
                        logger.LogInformation("Executed NotificationTemplateSeeder.SeedAsync");
                    }
                }
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, retryForAvailability);
                }
            }
        }
        return host;
    }
}