using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

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
                            <a href=""{{Link}}"" target=""_blank"" style='display:inline-block;background:#2e7d32;color:#fff !important;padding:12px 24px;font-size:18px;font-weight:600;text-decoration:none;border-radius:6px;border:1px solid #1b5e20;box-shadow:0 2px 6px rgba(0,0,0,0.2);'>Activate your account ➜</a>
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
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        var welcomePlId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == welcomePlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = welcomePlId,
                Subject = "Witamy w WasteFree!",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:40px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:32px;'>
                          <h2 style='color:#2e7d32;'>Witamy {{Username}} w WasteFree!</h2>
                          <p>
                            Dziękujemy za rejestrację. Cieszymy się, że dołączasz do naszej społeczności.<br><br>
                            <b>Rozpocznij, odkrywając nasze funkcje i ograniczając marnowanie już dziś!</b> <br><br>
                            <a href=""{{Link}}"" target=""_blank"" style='display:inline-block;background:#2e7d32;color:#fff !important;padding:12px 24px;font-size:18px;font-weight:600;text-decoration:none;border-radius:6px;border:1px solid #1b5e20;box-shadow:0 2px 6px rgba(0,0,0,0.2);'>Aktywuj swoje konto ➜</a>
                          </p>
                          <hr style='margin:24px 0;'>
                          <p style='font-size:12px;color:#888;'>
                            Jeśli to nie Ty się rejestrowałeś, zignoruj tę wiadomość.
                          </p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.RegisterationConfirmation,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }

        // New: Inbox / GarbageGroupInvitation templates (English)
        var invitationInboxEnId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == invitationInboxEnId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = invitationInboxEnId,
                Subject = "Invitation to join {{GroupName}}",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>{{SenderUsername}} invited you to join the group <strong>{{GroupName}}</strong>.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageGroupInvitation,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        // New: Inbox / GarbageGroupInvitation templates (Polish)
        var invitationInboxPlId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == invitationInboxPlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = invitationInboxPlId,
                Subject = "Zaproszenie do grupy {{GroupName}}",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>{{SenderUsername}} zaprosił Cię do grupy <strong>{{GroupName}}</strong>.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageGroupInvitation,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }
    }
}
