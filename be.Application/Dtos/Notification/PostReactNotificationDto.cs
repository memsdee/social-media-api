using be.Domain.Enums;

namespace be.Application.Dtos.Notification;

public class PostReactNotificationDto : NotificationDto
{
    public Guid PostId { get; set; }
    public ReactEnum ReactType { get; set; }
    public string? ThumbnailNoti { get; set; }
    public string? PreviewContent { get; set; }
    public bool IsGroup { get; set; }
    public short TotalCount { get; set; }
}