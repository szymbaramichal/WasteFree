using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Application.Jobs;

public class OneTimeJobs(IEmailService emailService)
{
    [TickerFunction(nameof(SendEmailJob))]
    public async Task SendEmailJob(TickerFunctionContext<SendEmailDto> sendEmailDto)
    {
        Console.WriteLine($"Starting SendEmailJob for {sendEmailDto.Request.Email}");
        await emailService.SendEmailAsync(sendEmailDto.Request.Email, sendEmailDto.Request.Subject, sendEmailDto.Request.Body);
        Console.WriteLine($"Ending SendEmailJob for {sendEmailDto.Request.Email}");
    }
}