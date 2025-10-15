using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using WasteFree.Infrastructure.Seeders;
using WasteFree.Infrastructure.Services;
using WasteFree.Shared.Interfaces;

namespace WasteFree.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerDatabase");
        services.AddDbContext<ApplicationDataContext>(opt => {
            opt.UseSqlite(connectionString);
        });

        services.AddTickerQ(opt =>
        {
            opt.AddOperationalStore<ApplicationDataContext>(efOptions =>
            {
                efOptions.UseModelCustomizerForMigrations();
                efOptions.CancelMissedTickersOnAppStart();
            });
            
            opt.AddDashboard(opt =>
            {
                opt.BasePath = "/tickerq";
                opt.EnableBasicAuth = true;
            });
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

        services.AddScoped<IEmailService>(_ =>
            new EmailService(
                smtpServer: smtpServer,
                smtpPort: int.Parse(smtpPortStr),
                smtpUser: smtpUser,
                smtpPass: smtpPass,
                from: "wastefreecloud@noreply.com"
            ));

        services.AddScoped<IJobSchedulerFacade, JobSchedulerFacade>();
        
        // Seeder registrations
        services.AddScoped<UserSeeder>();
        services.AddScoped<GarbageGroupSeeder>();
        services.AddScoped<UserGarbageGroupSeeder>();
        services.AddScoped<WalletTransactionSeeder>();
        services.AddScoped<InboxNotificationSeeder>();
        services.AddScoped<NotificationTemplateSeeder>();
        
        var blobConn = configuration["Integrations:BlobStorage:ConnectionString"];
        services.AddSingleton<IBlobStorageService>(_ => new AzureBlobStorageService(blobConn));
        
        return services;
    }

}