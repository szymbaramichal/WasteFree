using System.Net;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Wallet.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Wallet;

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
        TransactionType transactionType = TransactionType.Deposit;
        
        switch (paymentMethod.Type)
        {
            case nameof(TransactionType.Deposit):
                if (!request.PaymentProperties.StartsWith("777"))
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.InvalidTopupCode, HttpStatusCode.BadRequest);
                
                wallet.Funds += request.Amount;
                applicationDataContext.WalletTransactions.Add(new Domain.Entities.WalletTransaction
                {
                    Amount = request.Amount,
                    WalletId = wallet.Id,
                    TransactionType = Enum.TryParse<TransactionType>(paymentMethod.Type, true, out transactionType) 
                        ? transactionType 
                        : throw new InvalidOperationException("Invalid transaction type"),                
                });
        
                await applicationDataContext.SaveChangesAsync(cancellationToken);

                paymentTransactionDto.PaymentStatus = PaymentStatus.Completed;
                return Result<PaymentTransactionDto>.Success(paymentTransactionDto);
            
            case nameof(TransactionType.Withdrawal):
                if(string.IsNullOrEmpty(wallet.WithdrawalAccountNumber))
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.MissingAccountNumber, HttpStatusCode.BadRequest);
                
                if(wallet.Funds < request.Amount)
                    return Result<PaymentTransactionDto>.Failure(ApiErrorCodes.NotEnoughFunds, HttpStatusCode.BadRequest);
                    
                wallet.Funds -= request.Amount;
                applicationDataContext.WalletTransactions.Add(new Domain.Entities.WalletTransaction
                {
                    Amount = request.Amount,
                    WalletId = wallet.Id,
                    TransactionType = Enum.TryParse<TransactionType>(paymentMethod.Type, true, out transactionType) 
                        ? transactionType 
                        : throw new InvalidOperationException("Invalid transaction type"),    
                });
        
                await applicationDataContext.SaveChangesAsync(cancellationToken);

                paymentTransactionDto.PaymentStatus = PaymentStatus.Completed;
                return Result<PaymentTransactionDto>.Success(paymentTransactionDto);
            
            default:
                throw new NotImplementedException();
        }
    }
}