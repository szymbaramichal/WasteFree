using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Wallet.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Wallet;

public record GetWalletBalanceQuery(Guid UserId) : IRequest<BalanceDto>;

public class GetWalletBalanceQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetWalletBalanceQuery, BalanceDto>
{
    public async Task<Result<BalanceDto>> HandleAsync(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await context.Wallets
            .Where(x => x.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if(wallet is null)
            return Result<BalanceDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);

        return Result<BalanceDto>.Success(new BalanceDto(wallet.Funds));
    }
}