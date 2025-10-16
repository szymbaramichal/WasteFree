namespace WasteFree.Application.Jobs.Dtos;

public class SendEmailDto
{
    public required string Email { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}