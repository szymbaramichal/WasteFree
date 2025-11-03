using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Notifications.Facades;

namespace WasteFree.Application.Extensions;

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

        services.AddScoped<RegisterUserNotificationFacade>();
        services.AddScoped<GarbageGroupInvitationNotificationFacade>();
        services.AddScoped<GarbageOrderCreatedNotificationFacade>();
        
        return services;
    }

}