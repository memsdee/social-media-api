using be.Application.Dtos.EventBus;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Conversation;
using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface IConversationReadRepository
{
    Task AddAsync(ConversationDocument input, CancellationToken ct);
    Task<Guid?> GetSingleConversationIdAsync(long keyPart, CancellationToken ct);

    Task<Conversation1Dto?> GetSingleConversationAsync(Guid conversationPublicId, short userSequence,
        CancellationToken ct);

    Task<CursorResult<Conversation1Dto, CursorPayload<DateTimeOffset>?>> GetListConversationsAsync(
        short userSequenceId,
        int limit,
        CursorPayload<DateTimeOffset>? cursor,
        CancellationToken ct);

    Task<short?> GetIdByCheckConversationUserExistsAsync(Guid conversationPublicId, short privateUserId,
        CancellationToken ct);

    Task UpdateNewMessageAsync(MessageSentEvent input, CancellationToken ct);
    Task<int> GetTotalUnreadCountAsync(short userSequenceId, CancellationToken ct);
}