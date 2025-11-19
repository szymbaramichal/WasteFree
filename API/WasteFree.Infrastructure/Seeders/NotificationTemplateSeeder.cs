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

        var orderAcceptedEmailEnId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == orderAcceptedEmailEnId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = orderAcceptedEmailEnId,
                Subject = "Garbage order for {{GroupName}} accepted",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:32px;'>
                        <div style='max-width:640px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:28px;'>
                          <h2 style='color:#2e7d32;margin-bottom:16px;'>Order accepted</h2>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Hi {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Administrator <strong>{{GarbageAdminName}}</strong> accepted the garbage order for group <strong>{{GroupName}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Pickup is scheduled for <strong>{{PickupDate}}</strong>. You will receive updates once the collection is completed.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.GarbageOrderAccepted,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        var orderAcceptedEmailPlId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == orderAcceptedEmailPlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = orderAcceptedEmailPlId,
                Subject = "Zamówienie dla {{GroupName}} zaakceptowane",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:32px;'>
                        <div style='max-width:640px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:28px;'>
                          <h2 style='color:#2e7d32;margin-bottom:16px;'>Zamówienie zaakceptowane</h2>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Administrator <strong>{{GarbageAdminName}}</strong> zaakceptował zamówienie na wywóz dla grupy <strong>{{GroupName}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Odbiór zaplanowany jest na <strong>{{PickupDate}}</strong>. Poinformujemy Cię po zakończeniu wywozu.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Email,
                Type = NotificationType.GarbageOrderAccepted,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }

        var orderAcceptedInboxEnId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == orderAcceptedInboxEnId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = orderAcceptedInboxEnId,
                Subject = "Garbage order accepted",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Hi {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>{{GarbageAdminName}} accepted the garbage order for <strong>{{GroupName}}</strong> scheduled on <strong>{{PickupDate}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Track the order status in the app.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageOrderAccepted,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.English
            });
            await context.SaveChangesAsync();
        }

        var orderAcceptedInboxPlId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == orderAcceptedInboxPlId))
        {
            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = orderAcceptedInboxPlId,
                Subject = "Zamówienie zaakceptowane",
                Body = @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f9f9f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px #ccc;padding:20px;'>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:16px;color:#333;margin:0 0 12px;'>{{GarbageAdminName}} zaakceptował zamówienie dla <strong>{{GroupName}}</strong> zaplanowane na <strong>{{PickupDate}}</strong>.</p>
                          <p style='font-size:14px;color:#555;margin:0;'>Śledź status zamówienia w aplikacji.</p>
                        </div>
                      </body>
                    </html>",
                Channel = NotificationChannel.Inbox,
                Type = NotificationType.GarbageOrderAccepted,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = LanguagePreference.Polish
            });
            await context.SaveChangesAsync();
        }

        await SeedUtilizationFeeTemplatesAsync();
    }

    private async Task SeedUtilizationFeeTemplatesAsync()
    {
        var templates = new (Guid Id, string Subject, string Body, NotificationChannel Channel, NotificationType Type, LanguagePreference Language)[]
        {
            (
                Guid.Parse("99999999-9999-9999-9999-999999999901"),
                "Additional utilization fee required for {{GroupName}}",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f4f6f8;padding:20px;'>
                        <div style='max-width:560px;margin:auto;background:#ffffff;border-radius:10px;padding:24px;box-shadow:0 4px 16px rgba(0,0,0,0.08);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Hi {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>Order <strong>{{OrderId}}</strong> for group <strong>{{GroupName}}</strong> incurred additional utilization costs.</p>
                          <p style='font-size:14px;color:#475569;margin:0 0 16px;'>Outstanding amount: <strong>{{OutstandingAmount}} PLN</strong><br/>Your share: <strong>{{UserShare}} PLN</strong>.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>Please settle the fee in the app to help close the order.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeePending,
                LanguagePreference.English
            ),
            (
                Guid.Parse("99999999-9999-9999-9999-999999999902"),
                "Dodatkowa oplata utylizacyjna dla {{GroupName}}",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f4f6f8;padding:20px;'>
                        <div style='max-width:560px;margin:auto;background:#ffffff;border-radius:10px;padding:24px;box-shadow:0 4px 16px rgba(0,0,0,0.08);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>Zamówienie <strong>{{OrderId}}</strong> dla grupy <strong>{{GroupName}}</strong> wygenerowało dodatkowe koszty utylizacji.</p>
                          <p style='font-size:14px;color:#475569;margin:0 0 16px;'>Do zapłaty: <strong>{{OutstandingAmount}} PLN</strong><br/>Twoja część: <strong>{{UserShare}} PLN</strong>.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>Ureguluj opłatę w aplikacji, aby zamknąć zamówienie.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeePending,
                LanguagePreference.Polish
            ),
            (
                Guid.Parse("99999999-9999-9999-9999-999999999903"),
                "Settlement complete for {{GroupName}}",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f6f6f6;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 6px 20px rgba(15,23,42,0.1);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Hi {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>Order <strong>{{OrderId}}</strong> for <strong>{{GroupName}}</strong> has been fully settled.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>Thank you for completing all payments.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeeCompletedParticipant,
                LanguagePreference.English
            ),
            (
                Guid.Parse("99999999-9999-9999-9999-999999999904"),
                "Rozliczenie zamówienia {{GroupName}} zakończone",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f6f6f6;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 6px 20px rgba(15,23,42,0.1);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>Zamówienie <strong>{{OrderId}}</strong> dla grupy <strong>{{GroupName}}</strong> zostało w pełni rozliczone.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>Dziękujemy za terminową płatność.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeeCompletedParticipant,
                LanguagePreference.Polish
            ),
            (
                Guid.Parse("99999999-9999-9999-9999-999999999905"),
                "Participants settled utilization fee for {{GroupName}}",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f1f5f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 8px 24px rgba(15,23,42,0.12);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Hello {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>All participants paid the outstanding utilization fee for order <strong>{{OrderId}}</strong> in group <strong>{{GroupName}}</strong>.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>You can proceed with closing the process.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeeCompletedAdmin,
                LanguagePreference.English
            ),
            (
                Guid.Parse("99999999-9999-9999-9999-999999999906"),
                "Uczestnicy opłacili dodatkową opłatę dla {{GroupName}}",
                @"<html>
                      <body style='font-family:Arial,sans-serif;background:#f1f5f9;padding:20px;'>
                        <div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 8px 24px rgba(15,23,42,0.12);'>
                          <p style='font-size:16px;color:#0f172a;margin:0 0 12px;'>Cześć {{Username}},</p>
                          <p style='font-size:15px;color:#334155;margin:0 0 12px;'>Wszyscy uczestnicy opłacili dodatkową opłatę utylizacyjną dla zamówienia <strong>{{OrderId}}</strong> w grupie <strong>{{GroupName}}</strong>.</p>
                          <p style='font-size:14px;color:#475569;margin:0;'>Możesz zakończyć proces.</p>
                        </div>
                      </body>
                    </html>",
                NotificationChannel.Inbox,
                NotificationType.UtilizationFeeCompletedAdmin,
                LanguagePreference.Polish
            )
        };

        foreach (var template in templates)
        {
            if (await context.NotificationTemplates.AnyAsync(t => t.Id == template.Id))
            {
                continue;
            }

            context.NotificationTemplates.Add(new NotificationTemplate
            {
                Id = template.Id,
                Subject = template.Subject,
                Body = template.Body,
                Channel = template.Channel,
                Type = template.Type,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                LanguagePreference = template.Language
            });
            await context.SaveChangesAsync();
        }
    }
}
