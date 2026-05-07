using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace be.Infrastructure.Hubs;

[Authorize("user")]
public class MainHub : Hub
{
    public async Task JoinPost(Guid postId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, postId.ToString());
    }

    public async Task LeavePost(Guid postId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, postId.ToString());
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }
}