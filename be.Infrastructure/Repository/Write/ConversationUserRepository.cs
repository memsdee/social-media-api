using be.Application.Dtos.Queries.Conversation;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class ConversationUserRepository(WriteContext dbContext) : IConversationUserRepository
{
    public async Task AddRangeAsync(IEnumerable<ConversationUser> participants, CancellationToken ct)
    {
        await dbContext.ConversationUsers.AddRangeAsync(participants, ct);
    }

    public async Task UpdateUnreadAsync(short conversationPrivateId, short privateUserId, short unreadCount,
        CancellationToken ct)
    {
        await dbContext.ConversationUsers
            .Where(x => x.UserId == privateUserId && x.ConversationId == conversationPrivateId)
            .ExecuteUpdateAsync(c => c.SetProperty(z => z.UnreadCount, unreadCount), ct);
    }

    public async Task<bool> ExistsAsync(short conversationPrivateId, short privateUserId, CancellationToken ct)
    {
        return await dbContext.ConversationUsers
            .AsNoTracking()
            .AnyAsync(x => x.ConversationId == conversationPrivateId && x.UserId == privateUserId, ct);
    }

    public async Task IncrementUnreadForOthersAsync(short conversationPrivateId, short privateUserId,
        CancellationToken ct)
    {
        await dbContext.ConversationUsers
            .Where(x => x.ConversationId == conversationPrivateId && x.UserId != privateUserId)
            .ExecuteUpdateAsync(x => x.SetProperty(c => c.UnreadCount, c => (short)(c.UnreadCount + 1)), ct);
    }

    public async Task<ConversationUser1Dto[]> GetOthersAsync(short conversationPrivateId, short privateUserId,
        CancellationToken ct)
    {
        return await dbContext.ConversationUsers
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationPrivateId && x.UserId != privateUserId)
            .Select(x => new ConversationUser1Dto
            {
                PrivateUserId = x.UserId,
                UnreadCount = x.UnreadCount
            })
            .ToArrayAsync(ct);
    }
}