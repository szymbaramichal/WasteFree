using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using WasteFree.Api.Endpoints;
using WasteFree.Infrastructure.Hubs;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WasteFree.Api.Extensions;

public static class ProgramExtensions
{
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(opt =>
        {
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Description = "Paste your JWT Bearer token here",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            opt.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });

            foreach (var xmlFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml"))
            {
                try
                {
                    opt.IncludeXmlComments(xmlFile);
                }
                catch
                {
                    // ignored
                }
            }

            // Add schema filter that injects enum member descriptions (from XML comments) into schema.Description and x-enumDescriptions
            opt.SchemaFilter<WasteFree.Api.Swagger.EnumDescriptionsSchemaFilter>();
        });

        return services;
    }

    public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create("Shared", typeof(Program).Assembly.FullName!);
        });

        return services;
    }

    public static WebApplication UseSwaggerAndOpenApi(this WebApplication app, IConfiguration configuration)
    {
        app.UseSwagger();
        
        app.MapOpenApi();
        app.MapScalarApiReference("docs", x =>
        {
            x.Title = $"{configuration.GetValue<string>("applicationName")} Reference";
            x.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = new[] { "Bearer" }
            };
            x.OpenApiRoutePattern = "/swagger/v1/swagger.json";
            x.WithTheme(ScalarTheme.Kepler);
            x.Servers = new List<ScalarServer>
            {
                new ScalarServer(configuration["BaseApiUrl"] ?? string.Empty, "DEV server"),
            };
        });

        return app;
    }

    public static WebApplication UseRequestLocalizationSetup(this WebApplication app)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("pl-PL"),
            new CultureInfo("en-US")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CultureInfo.GetCultureInfo("pl-PL")),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders = new List<IRequestCultureProvider>()
            {
                new QueryStringRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            }
        });

        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapGarbageGroupsEndpoints();
        app.MapWalletEndpoints();
        app.MapAccountEndpoints();
        app.MapInboxEndpoints();
        app.MapCitiesEndpoints();
        app.MapConsentsEndpoints();
        app.MapGarbageGroupOrderEndpoints();

        return app;
    }

    public static WebApplication MapApplicationHubs(this WebApplication app, string allowLocalFrontendOrigins)
    {
        app.MapHub<NotificationHub>("/notificationHub")
           .RequireCors(allowLocalFrontendOrigins);

        return app;
    }
}
