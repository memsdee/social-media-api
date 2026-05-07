namespace be.Application.Interfaces.External;

public interface IRealtimeNotifier
{
    Task SendToUserAsync(string userId, string method, object? payload, CancellationToken ct);
    Task SendToGroupAsync(string group, string method, object? payload, CancellationToken ct = default);

    Task SendToGroupExceptAsync(string group, string excludeConnectionId, string method, object? payload = null,
        CancellationToken ct = default);
}