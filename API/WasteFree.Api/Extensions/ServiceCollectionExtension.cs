using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WasteFree.App.Services;
using WasteFree.Business.Extensions;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterLayers(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfrastructureServices(configuration);
        services.RegisterBusinessServices(configuration);

        return services;
    }

    public static IServiceCollection RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHierarchicalRolePolicies();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                var tokenKey = configuration["Security:Jwt:Key"] ?? throw new KeyNotFoundException("Token key not found");
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                    ValidateIssuer = false, // no issuer in token
                    ValidateAudience = false // no audience in token
                };
            });


        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
    
    public static IServiceCollection RegisterRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 40,
                        Window = TimeSpan.FromMinutes(1)
                    }
                )
            );
        });

        return services;
    }

    public static IServiceCollection RegisterCorsPolicy(this IServiceCollection services, string corsPolicyName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyName,
                policy  =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5000", "https://localhost:5000");
                });
        });
        
        return services;
    }
}