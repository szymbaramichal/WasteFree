using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WasteFree.Business.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

}