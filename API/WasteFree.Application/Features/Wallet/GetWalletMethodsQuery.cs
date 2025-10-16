using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Wallet;

public record GetWalletMethodsQuery : IRequest<IReadOnlyCollection<WalletMethod>>;

public class GetWalletMethodsQueryHandler : IRequestHandler<GetWalletMethodsQuery, IReadOnlyCollection<WalletMethod>>
{
    public async Task<Result<IReadOnlyCollection<WalletMethod>>> HandleAsync(GetWalletMethodsQuery request, CancellationToken cancellationToken)
    {
        var methods = WalletMethods.AvailableMethods;

        return Result<IReadOnlyCollection<WalletMethod>>.Success(methods);
    }
}