using WasteFree.Shared.Models;

namespace WasteFree.Business.Abstractions.Messaging;

public interface IRequest<TResponse> { }

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}