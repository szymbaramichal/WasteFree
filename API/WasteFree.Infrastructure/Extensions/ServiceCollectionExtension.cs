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
        
        var smtpServer = configuration["Integrations:Smtp:Server"];
        var smtpPortStr = configuration["Integrations:Smtp:Port"];
        var smtpUser = configuration["Integrations:Smtp:Username"];
        var smtpPass = configuration["Integrations:Smtp:Password"];

        // Cant validate, for local dev purposes 
        // if (string.IsNullOrWhiteSpace(smtpServer) ||
        //     string.IsNullOrWhiteSpace(smtpPortStr) ||
        //     string.IsNullOrWhiteSpace(smtpUser) ||
        //     string.IsNullOrWhiteSpace(smtpPass))
        // {
        //     throw new InvalidOperationException("Missing required SMTP configuration.");
        // }

        services.AddSingleton<IEmailService>(_ =>
            new EmailService(
                smtpServer: smtpServer,
                smtpPort: int.Parse(smtpPortStr),
                smtpUser: smtpUser,
                smtpPass: smtpPass,
                from: "wastefreecloud@noreply.com"
            ));

        return services;
    }

}