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

        var garbageOrderEmailEnId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == garbageOrderEmailEnId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = garbageOrderEmailEnId,
                Subject = "Your garbage order for {{GroupName}} is scheduled",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:32px;'>
                        <div style='max-width:640px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:28px;'>
                          <h2 style='color:#2e7d32;margin-bottom:16px;'>Garbage order created</h2>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Hello {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Your group <strong>{{GroupName}}</strong> created a garbage order.</p>
                          <p style='font-size:14px;color:#555;'>As you are member of that order, sign in to WasteFreeCloud portal and make payment in 7 days.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.GarbageOrderCreated,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        var garbageOrderEmailPlId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == garbageOrderEmailPlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = garbageOrderEmailPlId,
                Subject = "Zamowienie na odpady dla {{GroupName}} zostalo utworzone",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:32px;'>
                        <div style='max-width:640px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:28px;'>
                          <h2 style='color:#2e7d32;margin-bottom:16px;'>Zamówienie na odpady zostało utworzone</h2>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Twoja grupa <strong>{{GroupName}}</strong> utworzyła zamówienie na odpady.</p>
                          <p style='font-size:14px;color:#555;'>Jako, że jesteś członkiem tego wywozu to zaloguj się do portalu WasteFreeCloud i dokonaj płatności w ciągu 7 dni.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.GarbageOrderCreated,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }

        var garbageOrderInboxEnId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == garbageOrderInboxEnId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = garbageOrderInboxEnId,
                Subject = "Garbage order created",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Hi {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>A new garbage order for group <strong>{{GroupName}}</strong> is scheduled on <strong>{{PickupDate}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Check the order details and confirm your payment.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageOrderCreated,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        var garbageOrderInboxPlId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == garbageOrderInboxPlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = garbageOrderInboxPlId,
                Subject = "Zamowienie na odpady utworzone",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Czesc {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Nowe zamowienie dla grupy <strong>{{GroupName}}</strong> zaplanowane na <strong>{{PickupDate}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Sprawdz szczegoly zamowienia i potwierdz swoja płatność.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageOrderCreated,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }
    }
}
