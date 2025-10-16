using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WasteFree.Domain.Interfaces;

namespace WasteFree.Infrastructure.Hubs;
public class NotificationHub(ICurrentUserService currentUserService) : Hub
{
    private static readonly ConcurrentDictionary<Guid, string> UserConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = currentUserService.UserId;

        if (userId != Guid.Empty)
        {
            UserConnections[userId] = Context.ConnectionId;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = currentUserService.UserId;
        if (userId != Guid.Empty)
        {
            UserConnections.TryRemove(userId, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(Guid userId)
    {
        return UserConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }
}
