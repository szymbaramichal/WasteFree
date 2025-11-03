using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Helpers;
using WasteFree.Application.Notifications.Models;
using WasteFree.Domain.Enums;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Notifications.Facades;

public sealed class RegisterUserNotificationFacade(ApplicationDataContext context)
{
    public async Task<NotificationMessage?> CreateAsync(
        LanguagePreference languagePreference,
        string username,
        string activationLink,
        CancellationToken cancellationToken)
    {
        var template = await context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Type == NotificationType.RegisterationConfirmation
                     && t.Channel == NotificationChannel.Email
                     && t.LanguagePreference == languagePreference,
                cancellationToken);

        if (template is null)
        {
            return null;
        }

        var placeholders = new Dictionary<string, string>
        {
            ["Username"] = username,
            ["Link"] = activationLink
        };

        var subject = EmailTemplateHelper.ApplyPlaceholders(template.Subject, placeholders);
        var body = EmailTemplateHelper.ApplyPlaceholders(template.Body, placeholders);

        return new NotificationMessage(NotificationChannel.Email, subject, body);
    }
}
