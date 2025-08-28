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
        
        services.AddSingleton<IEmailService>(_ =>
            new EmailService(
                smtpServer: configuration["Integrations:Smtp:Server"] ?? string.Empty,
                smtpPort: int.TryParse(configuration["Integrations:Smtp:Port"], out var port) ? port : 25,
                smtpUser: configuration["Integrations:Smtp:Username"] ?? string.Empty,
                smtpPass: configuration["Integrations:Smtp:Password"] ?? string.Empty,
                from: "wastefreecloud@noreply.com"
            ));

        return services;
    }

}