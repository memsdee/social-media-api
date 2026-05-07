using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IConversationRepository
{
    Task<Guid?> GetSingleConversationPublicIdByKeyPartAsync(long keyPart, CancellationToken ct);
    Task AddAsync(Conversations conversation, CancellationToken ct);
    Task<short?> GetPrivateIdByPublicIdAsync(Guid conversationPublicId, CancellationToken ct);

    Task UpdateLastMessageAsync(short conversationPrivateId, string? lastMessage, DateTimeOffset updatedAt,
        CancellationToken ct);
}