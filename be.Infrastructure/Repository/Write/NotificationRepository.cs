using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class NotificationRepository(WriteContext dbContext) : INotificationRepository
{
    public async Task AddAsync(Notifications input, CancellationToken cancellationToken)
    {
        await dbContext.Notifications.AddAsync(input, cancellationToken);
    }

    public async Task MarkReadAsync(short[] notificationIds, short receiverId, DateTimeOffset readAt,
        CancellationToken ct)
    {
        await dbContext.Notifications
            .Where(x => notificationIds.Contains(x.Id) && x.ReciverId == receiverId && x.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ReadAt, readAt), ct);
    }

    public async Task DeleteByIdsAsync(List<short> notificationIds, CancellationToken ct)
    {
        await dbContext.Notifications.Where(x => notificationIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
    }

    public void Delete(Notifications notification)
    {
        dbContext.Notifications.Remove(notification);
    }

    public async Task<Notifications?> GetBySenderAndIdsAsync(short senderId, List<short> notificationIds,
        CancellationToken ct)
    {
        return await dbContext.Notifications.FirstOrDefaultAsync(
            x => x.SenderId == senderId && notificationIds.Contains(x.Id), ct);
    }
}