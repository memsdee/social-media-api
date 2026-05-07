using be.Application.Interfaces.External;
using be.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace be.Infrastructure.Services;

public class RealtimeNotifierService(IHubContext<MainHub> hubContext) : IRealtimeNotifier
{
    public Task SendToUserAsync(string userId, string method, object? payload, CancellationToken ct = default)
    {
        return hubContext.Clients.User(userId).SendAsync(method, payload, ct);
    }

    public Task SendToGroupAsync(string group, string method, object? payload, CancellationToken ct = default)
    {
        return hubContext.Clients.Group(group).SendAsync(method, payload, ct);
    }

    public Task SendToGroupExceptAsync(string group, string excludeConnectionId, string method, object? payload = null,
        CancellationToken ct = default)
    {
        return hubContext.Clients
            .GroupExcept(group, excludeConnectionId)
            .SendAsync(method, payload, ct);
    }
}