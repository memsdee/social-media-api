using be.Application.Dtos.Queries.Conversation;
using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IConversationUserRepository
{
    Task AddRangeAsync(IEnumerable<ConversationUser> participants, CancellationToken ct);
    Task UpdateUnreadAsync(short conversationPrivateId, short privateUserId, short unreadCount, CancellationToken ct);
    Task<bool> ExistsAsync(short conversationPrivateId, short privateUserId, CancellationToken ct);
    Task IncrementUnreadForOthersAsync(short conversationPrivateId, short privateUserId, CancellationToken ct);
    Task<ConversationUser1Dto[]> GetOthersAsync(short conversationPrivateId, short privateUserId, CancellationToken ct);
}