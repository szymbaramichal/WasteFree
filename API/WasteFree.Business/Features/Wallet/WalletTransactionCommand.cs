using System.Net;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Wallet.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Wallet;

public record WalletTransactionCommand(Guid UserId, string PaymentCode, double Amount, string PaymentProperties) 
    : IRequest<PaymentTransactionDto>;

public class WalletTransactionHandler(ApplicationDataContext applicationDataContext) : IRequestHandler<WalletTransactionCommand,PaymentTransactionDto>
{
    public async Task<Result<PaymentTransactionDto>> HandleAsync(WalletTransactionCommand request, 
        CancellationToken cancellationToken)
    {
        //Simulate some processing time
        await Task.Delay(1_000, cancellationToken);
        
        var paymentMethod = WalletMethods.AvailableMethods.First(x =>
            string.Equals(x.Code, request.PaymentCode, StringComparison.InvariantCultureIgnoreCase));
        
        PaymentTransactionDto paymentTransactionDto = new PaymentTransactionDto();
        var wallet = applicationDataContext.Wallets.First(x => x.UserId == request.UserId);
        
        switch (paymentMethod.Type)
        {
            case TransactionType.Deposit:
                if (!request.PaymentProperties.StartsWith("777"))
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.InvalidTopupCode, HttpStatusCode.BadRequest);
                
                wallet.Funds += request.Amount;
                applicationDataContext.WalletTransactions.Add(new Shared.Entities.WalletTransaction
                {
                    Amount = request.Amount,
                    WalletId = wallet.Id,
                    TransactionType = paymentMethod.Type
                });
        
                await applicationDataContext.SaveChangesAsync(cancellationToken);

                paymentTransactionDto.PaymentStatus = PaymentStatus.Completed;
                return Result<PaymentTransactionDto>.Success(paymentTransactionDto);
            
            case TransactionType.Withdrawal:
                if(string.IsNullOrEmpty(wallet.WithdrawalAccountNumber))
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.MissingAccountNumber, HttpStatusCode.BadRequest);
                
                if(wallet.Funds < request.Amount)
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.NotEnoughFunds, HttpStatusCode.BadRequest);
                    
                wallet.Funds -= request.Amount;
                applicationDataContext.WalletTransactions.Add(new Shared.Entities.WalletTransaction
                {
                    Amount = request.Amount,
                    WalletId = wallet.Id,
                    TransactionType = paymentMethod.Type
                });
        
                await applicationDataContext.SaveChangesAsync(cancellationToken);

                paymentTransactionDto.PaymentStatus = PaymentStatus.Completed;
                return Result<PaymentTransactionDto>.Success(paymentTransactionDto);
            
            default:
                throw new NotImplementedException();
        }
    }
}