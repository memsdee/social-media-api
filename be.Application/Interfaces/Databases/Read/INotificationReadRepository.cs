using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface INotificationReadRepository
{
    Task AddAsync(NotificationDocument input, CancellationToken ct);

    Task<List<NotificationDocument>> GetByReceiverAsync(
        short receiverSequenceId,
        DateTimeOffset? cursorCreatedAt,
        short? cursorId,
        int fetchLimit,
        CancellationToken ct);

    Task<Dictionary<short, UserDocument>> GetUsersBySequenceIdsAsync(
        IReadOnlyCollection<short> sequenceIds,
        CancellationToken ct);

    Task<Dictionary<short, Guid>> GetPostPublicIdsAsync(
        IReadOnlyCollection<short> postSequenceIds,
        CancellationToken ct);

    Task<Dictionary<short, Guid>> GetCommentPublicIdsAsync(
        IReadOnlyCollection<short> commentSequenceIds,
        CancellationToken ct);

    Task<Dictionary<short, int>> GetCommentUniqueSenderCountsAsync(
        short receiverSequenceId,
        CancellationToken ct);

    Task<int> GetFollowUniqueSenderCountAsync(
        short receiverSequenceId,
        CancellationToken ct);

    Task<long> GetUnreadCountAsync(short receiverSequenceId, CancellationToken ct);

    Task MarkReadAsync(short[] notificationIds, short receiverSequenceId, DateTimeOffset readAt, CancellationToken ct);
}