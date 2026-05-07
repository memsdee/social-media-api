namespace be.Application.Dtos.Notification;

public class UserFollowNotificationDto : NotificationDto
{
    public bool IsGroup { get; set; }
    public short TotalCount { get; set; }
}