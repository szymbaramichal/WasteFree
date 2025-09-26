using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features.Account.Dtos;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string BankAccountNumber { get; set; } = null!;
}

public static class ProfileDtoExtensions
{
    public static ProfileDto MapToProfileDto(this User user)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            BankAccountNumber = user.Wallet?.WithdrawalAccountNumber ?? string.Empty,
            Description = user.Description ?? string.Empty,
            Email = user.Email,
            Username = user.Username
        };
    }
}