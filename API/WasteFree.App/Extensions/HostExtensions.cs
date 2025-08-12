using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WasteFree.Infrastructure;

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
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    context.Database.Migrate();

                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
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