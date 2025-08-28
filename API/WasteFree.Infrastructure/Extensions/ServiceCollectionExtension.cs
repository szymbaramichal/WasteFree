using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WasteFree.Infrastructure.Services;
using WasteFree.Shared.Interfaces;

namespace WasteFree.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerDatabase");
        services.AddDbContext<ApplicationDataContext>(opt => {
            opt.UseSqlServer(connectionString);
        });
        
        services.AddSingleton<IEmailService>(sp =>
            new EmailService(
                smtpServer: configuration["Integrations:Smtp:Server"],
                smtpPort: int.Parse(configuration["Integrations:Smtp:Port"]),
                smtpUser: configuration["Integrations:Smtp:Username"],
                smtpPass: configuration["Integrations:Smtp:Password"],
                from: "wastefreecloud@noreply.com"
            ));

        return services;
    }

}