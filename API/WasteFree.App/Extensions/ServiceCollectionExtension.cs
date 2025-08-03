using WasteFree.Business.Extensions;
using WasteFree.Infrastructure.Extensions;

namespace WasteFree.App.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterLayers(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfrastructureServices(configuration);
        services.RegisterBusinessServices(configuration);
        
        return services;
    }
}