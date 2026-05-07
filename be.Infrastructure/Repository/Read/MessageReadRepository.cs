using be.Application.Dtos.EventBus;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Message;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class MessageReadRepository(ReadContext dbContext) : IMessageReadRepository
{
    public async Task AddAsync(MessageDocument input, CancellationToken ct)
    {
        await dbContext.Collection<MessageDocument>()
            .InsertOneAsync(input, null, ct);
    }

    public async Task<CursorResult<Message1Dto, CursorPayload<DateTimeOffset>?>> GetListMessage(short privateId,
        int limit, CursorPayload<DateTimeOffset>? cursor, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageDocument>.Filter.And(
            Builders<MessageDocument>.Filter.Eq(x => x.ConversationSequeceId, privateId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<MessageDocument, DateTimeOffset>(
                x => x.CreatedAt, x => x.SequenceId, cursor?.Selector, cursor?.Id)
        );

        var items = await dbContext.Collection<MessageDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new Message1Dto
            {
                ConversationId = x.ConversationId,
                Content = x.Content,
                CreatedAt = x.CreatedAt,
                SenderPublicId = x.SenderPublicId,
                Sequence = x.SequenceId
            })
            .ToListAsync(cancellationToken);

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreatedAt, x.Sequence));
    }

    public async Task<List<short>> GetListMessUnReadAsync(short privateConversaitonId, short privateUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Collection<MessageDocument>()
            .Find(x => x.ConversationSequeceId == privateConversaitonId &&
                       x.SeenBy.All(s => s.UserSequenceId != privateUserId))
            .Project(x => x.SequenceId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateReadMessage(MarkReadMessEvent input, CancellationToken cancellationToken)
    {
        await dbContext.Collection<MessageDocument>()
            .UpdateOneAsync(x => x.ConversationSequeceId == input.ConversationSequenceId,
                Builders<MessageDocument>.Update.AddToSet(x => x.SeenBy, new SeenBy
                {
                    UserSequenceId = input.UserSequenceId,
                    UserPublicId = input.UserPublicId,
                    ReadAt = input.ReadAt
                }), cancellationToken: cancellationToken);
    }
}