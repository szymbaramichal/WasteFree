using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using WasteFree.Infrastructure.Seeders;
using WasteFree.Infrastructure.Services;
using WasteFree.Domain.Interfaces;
using WasteFree.Infrastructure.Options;

namespace WasteFree.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDataContext>(opt => {
            opt.UseSqlite(connectionString);
        });

        services.Configure<NominatimOptions>(configuration.GetSection("Integrations:Nominatim"));

        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NominatimOptions>>().Value;

            var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                ? "https://nominatim.openstreetmap.org/"
                : options.BaseUrl;

            baseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

            client.BaseAddress = new Uri(baseUrl);

            var userAgent = string.IsNullOrWhiteSpace(options.UserAgent)
                ? "WasteFree/1.0 (+https://wastefreecloud.pl)"
                : options.UserAgent;

            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            if (!string.IsNullOrWhiteSpace(options.ContactEmail))
            {
                client.DefaultRequestHeaders.From = options.ContactEmail;
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
                smtpServer: smtpServer!,
                smtpPort: int.Parse(smtpPortStr!),
                smtpUser: smtpUser!,
                smtpPass: smtpPass!,
                from: "wastefreecloud@noreply.com"
            ));

        services.AddScoped<IJobSchedulerFacade, JobSchedulerFacade>();
        
        // Seeder registrations
        services.AddScoped<UserSeeder>();
        services.AddScoped<GarbageGroupSeeder>();
        services.AddScoped<UserGarbageGroupSeeder>();
        services.AddScoped<GarbageOrderSeeder>();
        services.AddScoped<GarbageOrderUsersSeeder>();
        services.AddScoped<WalletTransactionSeeder>();
        services.AddScoped<InboxNotificationSeeder>();
        services.AddScoped<NotificationTemplateSeeder>();
        services.AddScoped<GarbageAdminConsentSeeder>();
        
        var blobConn = configuration.GetConnectionString("BlobStorage");
        services.AddSingleton<IBlobStorageService>(_ => new AzureBlobStorageService(blobConn!));
        
        return services;
    }

}