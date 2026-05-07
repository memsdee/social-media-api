using be.Application.Dtos.EventBus;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Message;
using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface IMessageReadRepository
{
    Task AddAsync(MessageDocument input, CancellationToken ct);

    Task<CursorResult<Message1Dto, CursorPayload<DateTimeOffset>?>> GetListMessage(short privateId, int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken cancellationToken);

    Task<List<short>> GetListMessUnReadAsync(short privateConversaitonId, short privateUserId,
        CancellationToken cancellationToken);

    Task UpdateReadMessage(MarkReadMessEvent input, CancellationToken cancellationToken);
}