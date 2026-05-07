using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class ConversationRepository(WriteContext dbContext) : IConversationRepository
{
    public async Task<Guid?> GetSingleConversationPublicIdByKeyPartAsync(long keyPart, CancellationToken ct)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.Type == TypeConversationEnum.single && x.KeyParticipants == keyPart)
            .Select(x => (Guid?)x.IdPublic)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Conversations conversation, CancellationToken ct)
    {
        await dbContext.Conversations.AddAsync(conversation, ct);
    }

    public async Task<short?> GetPrivateIdByPublicIdAsync(Guid conversationPublicId, CancellationToken ct)
    {
        return await dbContext.Conversations.AsNoTracking()
            .Where(x => x.IdPublic == conversationPublicId)
            .Select(x => (short?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateLastMessageAsync(short conversationPrivateId, string? lastMessage, DateTimeOffset updatedAt,
        CancellationToken ct)
    {
        await dbContext.Conversations
            .Where(x => x.Id == conversationPrivateId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(v => v.LastMessage, lastMessage)
                .SetProperty(v => v.UpdatedAt, updatedAt), ct);
    }
}