using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WasteFree.Business.Abstractions.Messaging;

namespace WasteFree.Business.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMediator, Mediator>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IRequest<>))
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );
        
        return services;
    }

}