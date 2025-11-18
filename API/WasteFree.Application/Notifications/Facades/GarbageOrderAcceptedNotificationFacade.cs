using System;
using System.Collections.Generic;
using System.Globalization;
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

public sealed record GarbageOrderAcceptedNotificationRequest(
    Guid UserId,
    LanguagePreference LanguagePreference,
    string Username,
    string GroupName,
    DateTime PickupDate,
    string GarbageAdminName);

public sealed record GarbageOrderAcceptedNotificationContent(
    Guid UserId,
    NotificationMessage? Email,
    NotificationMessage? Inbox);

public sealed class GarbageOrderAcceptedNotificationFacade(ApplicationDataContext context)
{
    public async Task<IReadOnlyList<GarbageOrderAcceptedNotificationContent>> CreateAsync(
        IEnumerable<GarbageOrderAcceptedNotificationRequest> requests,
        CancellationToken cancellationToken)
    {
        var requestList = requests.ToList();
        if (requestList.Count == 0)
        {
            return [];
        }

        var languages = requestList
            .Select(r => r.LanguagePreference)
            .Distinct()
            .ToList();

        var templates = await context.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.Type == NotificationType.GarbageOrderAccepted
                        && languages.Contains(t.LanguagePreference))
            .ToListAsync(cancellationToken);

        var templatesByLanguage = templates
            .GroupBy(t => t.LanguagePreference)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<GarbageOrderAcceptedNotificationContent>(requestList.Count);

        foreach (var request in requestList)
        {
            templatesByLanguage.TryGetValue(request.LanguagePreference, out var languageTemplates);
            var placeholders = BuildPlaceholders(request);

            NotificationMessage? email = null;
            NotificationMessage? inbox = null;

            if (languageTemplates is not null)
            {
                email = CreateMessage(languageTemplates, NotificationChannel.Email, placeholders);
                inbox = CreateMessage(languageTemplates, NotificationChannel.Inbox, placeholders);
            }

            results.Add(new GarbageOrderAcceptedNotificationContent(request.UserId, email, inbox));
        }

        return results;
    }

    private static NotificationMessage? CreateMessage(
        IEnumerable<NotificationTemplate> templates,
        NotificationChannel channel,
        IReadOnlyDictionary<string, string> placeholders)
    {
        var template = templates.FirstOrDefault(t => t.Channel == channel);
        return template is null ? null : BuildMessage(template, placeholders);
    }

    private static NotificationMessage BuildMessage(
        NotificationTemplate template,
        IReadOnlyDictionary<string, string> placeholders)
    {
        var subject = EmailTemplateHelper.ApplyPlaceholders(template.Subject, placeholders);
        var body = EmailTemplateHelper.ApplyPlaceholders(template.Body, placeholders);
        return new NotificationMessage(template.Channel, subject, body);
    }

    private static IReadOnlyDictionary<string, string> BuildPlaceholders(
        GarbageOrderAcceptedNotificationRequest request)
    {
        return new Dictionary<string, string>
        {
            ["Username"] = request.Username,
            ["GroupName"] = request.GroupName,
            ["PickupDate"] = request.PickupDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
            ["GarbageAdminName"] = request.GarbageAdminName
        };
    }
}
