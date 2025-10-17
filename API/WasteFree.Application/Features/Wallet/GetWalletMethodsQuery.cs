using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Wallet;

public record GetWalletMethodsQuery : IRequest<IReadOnlyCollection<WalletMethod>>;

public class GetWalletMethodsQueryHandler : IRequestHandler<GetWalletMethodsQuery, IReadOnlyCollection<WalletMethod>>
{
    public async Task<Result<IReadOnlyCollection<WalletMethod>>> HandleAsync(GetWalletMethodsQuery request, CancellationToken cancellationToken)
    {
        var methods = WalletMethods.AvailableMethods;

        return Result<IReadOnlyCollection<WalletMethod>>.Success(methods);
    }
}