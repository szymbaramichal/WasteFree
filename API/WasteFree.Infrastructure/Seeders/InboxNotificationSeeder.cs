using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class InboxNotificationSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var notifications = new[]
        {
            new
            {
                Username = "test3",
                Title = "Zaproszenie do grupy",
                Message = "garbageadmin1 zaprosił Cię do grupy Kraków Centrum Recycling.",
                ActionType = InboxActionType.GroupInvitation,
                RelatedEntityId = (Guid?)Guid.Parse("55555555-5555-5555-5555-555555555555")
            },
            new
            {
                Username = "test2",
                Title = "Przypomnienie o płatności",
                Message = "Twoja płatność za odbiór odpadów jest wymagana do końca tygodnia.",
                ActionType = InboxActionType.MakePayment,
                RelatedEntityId = (Guid?)null
            },
            new
            {
                Username = "garbageadmin2",
                Title = "Nowy członek oczekuje",
                Message = "test3 oczekuje na zatwierdzenie dołączenia do grupy Gdańsk Eco Harbor.",
                ActionType = InboxActionType.GroupInvitation,
                RelatedEntityId = (Guid?)Guid.Parse("77777777-7777-7777-7777-777777777777")
            }
        };

        var usernames = notifications.Select(n => n.Username).Distinct().ToArray();
        var usersLookup = await context.Users
            .Where(u => usernames.Contains(u.Username))
            .ToDictionaryAsync(u => u.Username, u => u.Id);

        var changesMade = false;
        foreach (var notification in notifications)
        {
            if (!usersLookup.TryGetValue(notification.Username, out var userId))
            {
                continue;
            }

            var alreadySeeded = await context.InboxNotifications.AnyAsync(n =>
                n.UserId == userId && n.Title == notification.Title && n.Message == notification.Message);

            if (alreadySeeded)
            {
                continue;
            }

            await context.InboxNotifications.AddAsync(new InboxNotification
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Title = notification.Title,
                Message = notification.Message,
                ActionType = notification.ActionType,
                RelatedEntityId = notification.RelatedEntityId
            });
            changesMade = true;
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
