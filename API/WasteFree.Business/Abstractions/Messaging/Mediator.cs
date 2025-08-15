using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Abstractions.Messaging;

public interface IMediator
{
    Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<object>>> _handlersCache = new();

    public async Task<Result<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var invoker = _handlersCache.GetOrAdd(requestType, type =>
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(type, typeof(TResponse));
            var handleMethod = handlerType.GetMethod("HandleAsync");

            return async (sp, req, ct) =>
            {
                using var scope = sp.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                var task = (Task)handleMethod.Invoke(handler, new[] { req, ct });
                await task;
                return ((dynamic)task).Result;
            };
        });

        var result = (Result<TResponse>)await invoker(serviceProvider, request, cancellationToken);
        return result;
    }
}