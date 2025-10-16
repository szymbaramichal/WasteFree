using MailKit.Net.Smtp;
using MimeKit;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Infrastructure.Services;

public class EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPass, string from)
    : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("WasteFree Cloud", from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            await client.DisconnectAsync(true);
            Console.WriteLine(e);
        }
    }
}