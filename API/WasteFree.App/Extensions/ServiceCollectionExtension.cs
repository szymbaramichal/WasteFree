using System.Text;
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
        services.AddAuthorization();
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
}