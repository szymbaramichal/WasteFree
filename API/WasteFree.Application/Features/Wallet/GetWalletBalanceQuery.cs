using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Wallet.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Wallet;

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

        return Result<BalanceDto>.Success(new BalanceDto
        {
            Amount = wallet.Funds
        });
    }
}