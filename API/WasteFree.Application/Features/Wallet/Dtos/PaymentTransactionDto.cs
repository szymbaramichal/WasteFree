using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.Wallet.Dtos;

/// <summary>
/// DTO representing a payment transaction and its status.
/// </summary>
public class PaymentTransactionDto
{
    /// <summary>
    /// Current status of the payment transaction (for example Success, Failed, Pending).
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }
}