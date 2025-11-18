using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Helpers;
using WasteFree.Application.Notifications.Models;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Notifications.Facades;

public sealed record UtilizationFeeCompletionNotificationRequest(
    Guid UserId,
    LanguagePreference LanguagePreference,
    string Username,
    string GroupName,
    Guid OrderId,
    NotificationType TemplateType);

public sealed class UtilizationFeeCompletionNotificationFacade(ApplicationDataContext context)
{
    public async Task<IReadOnlyList<UtilizationFeeNotificationContent>> CreateAsync(
        IEnumerable<UtilizationFeeCompletionNotificationRequest> requests,
        CancellationToken cancellationToken)
    {
        var requestList = requests.ToList();
        if (requestList.Count == 0)
        {
            return [];
        }

        var requestTypes = requestList
            .Select(r => r.TemplateType)
            .Distinct()
            .ToList();

        var languages = requestList
            .Select(r => r.LanguagePreference)
            .Distinct()
            .ToList();

        var templates = await context.NotificationTemplates
            .AsNoTracking()
            .Where(t => requestTypes.Contains(t.Type)
                        && languages.Contains(t.LanguagePreference))
            .ToListAsync(cancellationToken);

        var templatesLookup = templates
            .GroupBy(t => (t.Type, t.LanguagePreference))
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<UtilizationFeeNotificationContent>(requestList.Count);

        foreach (var request in requestList)
        {
            templatesLookup.TryGetValue((request.TemplateType, request.LanguagePreference), out var templateSet);

            NotificationMessage? email = null;
            NotificationMessage? inbox = null;

            if (templateSet is not null)
            {
                var placeholders = BuildPlaceholders(request);
                email = CreateMessage(templateSet, NotificationChannel.Email, placeholders);
                inbox = CreateMessage(templateSet, NotificationChannel.Inbox, placeholders);
            }

            results.Add(new UtilizationFeeNotificationContent(request.UserId, email, inbox));
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
        UtilizationFeeCompletionNotificationRequest request)
    {
        return new Dictionary<string, string>
        {
            ["Username"] = request.Username,
            ["GroupName"] = request.GroupName,
            ["OrderId"] = request.OrderId.ToString()
        };
    }
}
