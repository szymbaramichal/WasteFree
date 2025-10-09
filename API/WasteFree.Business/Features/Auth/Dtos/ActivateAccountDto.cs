namespace WasteFree.Business.Features.Auth.Dtos;

/// <summary>
/// DTO used to activate a user account by Id.
/// </summary>
public class ActivateAccountDto
{
    /// <summary>
    /// Identifier of the account to activate.
    /// </summary>
    public Guid Id { get; set; }
}