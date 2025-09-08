namespace WasteFree.Business.Features.Account.Dtos;

public class ProfileDto
{
    public Guid UserId { get; set; }    
    public string Username { get; set; }
    public string Email { get; set; }
    public string Description { get; set; }
    public string BankAccountNumber { get; set; }
}