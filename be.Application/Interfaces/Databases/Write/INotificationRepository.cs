using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface INotificationRepository
{
    Task AddAsync(Notifications input, CancellationToken cancellationToken);
    Task MarkReadAsync(short[] notificationIds, short receiverId, DateTimeOffset readAt, CancellationToken ct);
    Task DeleteByIdsAsync(List<short> notificationIds, CancellationToken ct);
    void Delete(Notifications notification);
    Task<Notifications?> GetBySenderAndIdsAsync(short senderId, List<short> notificationIds, CancellationToken ct);
}