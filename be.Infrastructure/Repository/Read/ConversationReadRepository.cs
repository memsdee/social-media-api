using be.Application.Dtos.EventBus;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Conversation;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Domain.Enums;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class ConversationReadRepository(ReadContext dbContext) : IConversationReadRepository
{
    public async Task AddAsync(ConversationDocument input, CancellationToken ct)
    {
        await dbContext.Collection<ConversationDocument>()
            .InsertOneAsync(input, null, ct);
    }

    public async Task UpdateNewMessageAsync(MessageSentEvent input, CancellationToken ct)
    {
        var filter = Builders<ConversationDocument>.Filter.Eq(x => x.SequenceId, input.ConversationSequeceId);

        var update = Builders<ConversationDocument>.Update
            .Set(x => x.PreviewLastMess, input.PreviewLastMess)
            .Set(x => x.LastMessageDate, input.CreatedAt)
            .Set(x => x.LastUpdate, input.CreatedAt)
            .Inc("Participants.$[elem].UnreadCount", 1);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.UserSequenceId",
                new BsonDocument("$ne", input.SenderSequenceId)))
        };

        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

        await dbContext.Collection<ConversationDocument>().UpdateOneAsync(filter, update, updateOptions, ct);
    }

    public async Task<Guid?> GetSingleConversationIdAsync(long keyPart, CancellationToken ct)
    {
        return await dbContext.Collection<ConversationDocument>()
            .Find(x => x.KeyPart == keyPart && x.Type == TypeConversationEnum.single)
            .Project(x => (Guid?)x.IdPublic)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Conversation1Dto?> GetSingleConversationAsync(Guid conversationPublicId, short userSequence,
        CancellationToken ct)
    {
        return await dbContext.Collection<ConversationDocument>()
            .Find(x => x.Type == TypeConversationEnum.single && x.IdPublic == conversationPublicId)
            .Project(x => new Conversation1Dto
            {
                SequenceId = x.SequenceId,
                ConversationPublicId = x.IdPublic,
                UserName = x.Participants.Where(c => c.UserSequenceId != userSequence).Select(c => c.UserName)
                    .FirstOrDefault()!,
                Avatar = x.Participants.Where(c => c.UserSequenceId != userSequence).Select(c => (Guid?)c.UserAvatar)
                    .FirstOrDefault(),
                LastMessage = x.PreviewLastMess,
                LastMessageDate = x.LastMessageDate,
                LastUpdate = x.LastUpdate,
                UnreadCount = x.Participants.Where(c => c.UserSequenceId == userSequence).Select(c => c.UnreadCount)
                    .FirstOrDefault()
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<CursorResult<Conversation1Dto, CursorPayload<DateTimeOffset>?>> GetListConversationsAsync(
        short userSequenceId,
        int limit,
        CursorPayload<DateTimeOffset>? cursor,
        CancellationToken ct)
    {
        var filter = Builders<ConversationDocument>.Filter.And(
            Builders<ConversationDocument>.Filter.ElemMatch(
                x => x.Participants,
                p => p.UserSequenceId == userSequenceId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<ConversationDocument, DateTimeOffset>(
                x => x.LastUpdate,
                x => x.SequenceId,
                cursor?.Selector,
                cursor?.Id)
        );

        var items = await dbContext.Collection<ConversationDocument>()
            .Find(filter)
            .SortByDescending(x => x.LastUpdate)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new Conversation1Dto
            {
                SequenceId = x.SequenceId,
                ConversationPublicId = x.IdPublic,
                UserName = x.Participants
                    .Where(p => p.UserSequenceId != userSequenceId)
                    .Select(p => p.UserName)
                    .FirstOrDefault()!,
                Avatar = x.Participants
                    .Where(p => p.UserSequenceId != userSequenceId)
                    .Select(p => (Guid?)p.UserAvatar)
                    .FirstOrDefault(),
                LastMessage = x.PreviewLastMess,
                LastMessageDate = x.LastMessageDate,
                LastUpdate = x.LastUpdate,
                UnreadCount = x.Participants
                    .Where(p => p.UserSequenceId == userSequenceId)
                    .Select(p => p.UnreadCount)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return ReadCursorPagiCaculHelper.Paginate(
            items,
            limit,
            x => new CursorPayload<DateTimeOffset>(x.LastUpdate, x.SequenceId));
    }

    public async Task<short?> GetIdByCheckConversationUserExistsAsync(Guid conversationPublicId, short privateUserId,
        CancellationToken ct)
    {
        return await dbContext.Collection<ConversationDocument>()
            .Find(x => x.IdPublic == conversationPublicId && x.Participants.Any(p => p.UserSequenceId == privateUserId))
            .Project(x => (short?)x.SequenceId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetTotalUnreadCountAsync(short userSequenceId, CancellationToken ct)
    {
        return (await dbContext.Collection<ConversationDocument>()
            .Aggregate<BsonDocument>(new[]
            {
                new BsonDocument("$match", new BsonDocument("Participants.UserSequenceId", userSequenceId)),
                new BsonDocument("$unwind", "$Participants"),
                new BsonDocument("$match", new BsonDocument("Participants.UserSequenceId", userSequenceId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$Participants.UnreadCount") }
                })
            })
            .FirstOrDefaultAsync(ct))?.GetValue("total", 0).ToInt32() ?? 0;
    }
}