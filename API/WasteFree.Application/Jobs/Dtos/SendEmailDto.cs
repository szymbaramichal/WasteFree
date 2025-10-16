namespace WasteFree.Business.Jobs.Dtos;

public class SendEmailDto
{
    public required string Email { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}