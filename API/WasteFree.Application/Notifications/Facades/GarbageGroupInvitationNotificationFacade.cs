using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Helpers;
using WasteFree.Application.Notifications.Models;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Notifications.Facades;

public sealed record GarbageGroupInvitationNotificationContent(
    NotificationMessage? Email,
    NotificationMessage? Inbox);

public sealed class GarbageGroupInvitationNotificationFacade(ApplicationDataContext context)
{
    public async Task<GarbageGroupInvitationNotificationContent?> CreateAsync(
        LanguagePreference languagePreference,
        string recipientUsername,
        string senderUsername,
        string groupName,
        CancellationToken cancellationToken)
    {
        var templates = await context.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.Type == NotificationType.GarbageGroupInvitation
                        && t.LanguagePreference == languagePreference)
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            return null;
        }

        var placeholders = new Dictionary<string, string>
        {
            ["RecipientUsername"] = recipientUsername,
            ["SenderUsername"] = senderUsername,
            ["GroupName"] = groupName
        };

        var inboxTemplate = templates.FirstOrDefault(t => t.Channel == NotificationChannel.Inbox);
        var emailTemplate = templates.FirstOrDefault(t => t.Channel == NotificationChannel.Email) ?? inboxTemplate;

        var email = emailTemplate != null ? BuildMessage(emailTemplate, placeholders) : null;
        var inbox = inboxTemplate != null ? BuildMessage(inboxTemplate, placeholders) : null;

        if (email is null && inbox is null)
        {
            return null;
        }

        return new GarbageGroupInvitationNotificationContent(email, inbox);
    }

    private static NotificationMessage BuildMessage(
        NotificationTemplate template,
        IReadOnlyDictionary<string, string> placeholders)
    {
        var subject = EmailTemplateHelper.ApplyPlaceholders(template.Subject, placeholders);
        var body = EmailTemplateHelper.ApplyPlaceholders(template.Body, placeholders);
        return new NotificationMessage(template.Channel, subject, body);
    }
}
