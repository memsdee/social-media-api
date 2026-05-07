using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Domain.Enums;
using be.Infrastructure.Database;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class NotificationReadRepository(ReadContext dbContext) : INotificationReadRepository
{
    public async Task AddAsync(NotificationDocument input, CancellationToken ct)
    {
        await dbContext.Collection<NotificationDocument>()
            .InsertOneAsync(input, null, ct);
    }

    public async Task<List<NotificationDocument>> GetByReceiverAsync(
        short receiverSequenceId,
        DateTimeOffset? cursorCreatedAt,
        short? cursorId,
        int fetchLimit,
        CancellationToken ct)
    {
        var baseFilter = Builders<NotificationDocument>.Filter.Eq(x => x.ReceiveSequenceId, receiverSequenceId);
        var filter = baseFilter;

        if (cursorCreatedAt.HasValue)
        {
            var cursorFilter = Builders<NotificationDocument>.Filter.Lt(x => x.CreateAt, cursorCreatedAt.Value);

            if (cursorId.HasValue)
                cursorFilter = Builders<NotificationDocument>.Filter.Or(
                    cursorFilter,
                    Builders<NotificationDocument>.Filter.And(
                        Builders<NotificationDocument>.Filter.Eq(x => x.CreateAt, cursorCreatedAt.Value),
                        Builders<NotificationDocument>.Filter.Lt(x => x.SequenceId, cursorId.Value)
                    )
                );

            filter = Builders<NotificationDocument>.Filter.And(baseFilter, cursorFilter);
        }

        return await dbContext.Collection<NotificationDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreateAt)
            .ThenByDescending(x => x.SequenceId)
            .Limit(fetchLimit)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<short, UserDocument>> GetUsersBySequenceIdsAsync(
        IReadOnlyCollection<short> sequenceIds,
        CancellationToken ct)
    {
        if (sequenceIds.Count == 0)
            return new Dictionary<short, UserDocument>();

        var users = await dbContext.Collection<UserDocument>()
            .Find(x => sequenceIds.Contains(x.SequenceId))
            .ToListAsync(ct);

        return users
            .GroupBy(x => x.SequenceId)
            .ToDictionary(x => x.Key, x => x.First());
    }

    public async Task<Dictionary<short, Guid>> GetPostPublicIdsAsync(
        IReadOnlyCollection<short> postSequenceIds,
        CancellationToken ct)
    {
        if (postSequenceIds.Count == 0)
            return new Dictionary<short, Guid>();

        var posts = await dbContext.Collection<PostDocument>()
            .Find(x => postSequenceIds.Contains(x.SequenceId))
            .Project(x => new { x.SequenceId, x.IdPublic })
            .ToListAsync(ct);

        return posts.ToDictionary(x => x.SequenceId, x => x.IdPublic);
    }

    public async Task<Dictionary<short, Guid>> GetCommentPublicIdsAsync(
        IReadOnlyCollection<short> commentSequenceIds,
        CancellationToken ct)
    {
        if (commentSequenceIds.Count == 0)
            return new Dictionary<short, Guid>();

        var comments = await dbContext.Collection<CommentDocument>()
            .Find(x => commentSequenceIds.Contains(x.SequenceId))
            .Project(x => new { x.SequenceId, x.IdPublic })
            .ToListAsync(ct);

        return comments.ToDictionary(x => x.SequenceId, x => x.IdPublic);
    }

    public async Task<Dictionary<short, int>> GetCommentUniqueSenderCountsAsync(
        short receiverSequenceId,
        CancellationToken ct)
    {
        var filter = Builders<NotificationDocument>.Filter.And(
            Builders<NotificationDocument>.Filter.Eq(x => x.ReceiveSequenceId, receiverSequenceId),
            Builders<NotificationDocument>.Filter.Eq(x => x.Action, NotiActionEnum.comment),
            Builders<NotificationDocument>.Filter.Eq(x => x.Target, NotiTargetEnum.post),
            Builders<NotificationDocument>.Filter.Ne(x => x.PostSequenceId, null)
        );

        var items = await dbContext.Collection<NotificationDocument>()
            .Find(filter)
            .Project(x => new { PostSquenceId = x.PostSequenceId, SenderSquenceId = x.SenderSequenceId })
            .ToListAsync(ct);

        return items
            .Where(x => x.PostSquenceId.HasValue)
            .GroupBy(x => x.PostSquenceId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.Select(v => v.SenderSquenceId).Distinct().Count());
    }

    public async Task<int> GetFollowUniqueSenderCountAsync(
        short receiverSequenceId,
        CancellationToken ct)
    {
        return (await dbContext.Collection<NotificationDocument>()
            .Find(Builders<NotificationDocument>.Filter.And(
                Builders<NotificationDocument>.Filter.Eq(x => x.ReceiveSequenceId, receiverSequenceId),
                Builders<NotificationDocument>.Filter.Eq(x => x.Action, NotiActionEnum.follow),
                Builders<NotificationDocument>.Filter.Eq(x => x.Target, NotiTargetEnum.user)
            ))
            .Project(x => x.SenderSequenceId)
            .ToListAsync(ct)).Distinct().Count();
    }

    public async Task<long> GetUnreadCountAsync(short receiverSequenceId, CancellationToken ct)
    {
        return await dbContext.Collection<NotificationDocument>()
            .CountDocumentsAsync(Builders<NotificationDocument>.Filter.And(
                Builders<NotificationDocument>.Filter.Eq(x => x.ReceiveSequenceId, receiverSequenceId),
                Builders<NotificationDocument>.Filter.Eq(x => x.ReadAt, null)
            ), cancellationToken: ct);
    }

    public async Task MarkReadAsync(short[] notificationIds, short receiverSequenceId, DateTimeOffset readAt,
        CancellationToken ct)
    {
        var filter = Builders<NotificationDocument>.Filter.And(
            Builders<NotificationDocument>.Filter.In(x => x.SequenceId, notificationIds),
            Builders<NotificationDocument>.Filter.Eq(x => x.ReceiveSequenceId, receiverSequenceId),
            Builders<NotificationDocument>.Filter.Eq(x => x.ReadAt, null)
        );

        var update = Builders<NotificationDocument>.Update.Set(x => x.ReadAt, readAt);

        await dbContext.Collection<NotificationDocument>().UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}