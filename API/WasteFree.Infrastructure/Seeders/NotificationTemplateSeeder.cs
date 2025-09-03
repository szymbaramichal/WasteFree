using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class NotificationTemplateSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var welcomeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == welcomeId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = welcomeId,
                Subject = "Welcome to WasteFree!",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:40px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:32px;'>
                          <h2 style='color:#2e7d32;'>Welcome {{Username}} to WasteFree!</h2>
                          <p>
                            Thank you for registering. We are excited to have you join our community.<br><br>
                            <b>Get started by exploring our features and reducing waste today!</b> <br><br>
                            Activation link: <a href=""{{Link}}"" style='color:#2e7d32;text-decoration:underline;' target=""_blank"">Activate your account</a>
                          </p>
                          <hr style='margin:24px 0;'>
                          <p style='font-size:12px;color:#888;'>
                            If you did not register, please ignore this email.
                          </p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.RegisterationConfirmation,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            });
            await context.SaveChangesAsync();
        }
    }
}
