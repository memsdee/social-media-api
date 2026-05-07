namespace be.Application.Dtos.Notification;

public class PostCommentNotificationDto : NotificationDto
{
    public Guid PostId { get; set; }
    public Guid CommentId { get; set; }
    public string? ThumbnailNoti { get; set; }
    public string PreviewContent { get; set; } = null!;
    public bool IsGroup { get; set; }
    public short TotalCount { get; set; }
}