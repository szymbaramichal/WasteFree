using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WasteFree.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerDatabase");
        services.AddDbContext<ApplicationDataContext>(opt => {
            opt.UseSqlServer(connectionString);
        });

        return services;
    }

}