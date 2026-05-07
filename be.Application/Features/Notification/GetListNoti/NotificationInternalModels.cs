using be.Domain.Enums;

namespace be.Application.Features.Notification.GetListNoti;

internal class NotificationBaseRow
{
    public short Id { get; set; }
    public short SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? SenderAvatar { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public string? PreviewContent { get; set; }
    public string GroupKey { get; set; } = string.Empty;
}

internal class NotificationRawDto
{
    public short Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? SenderAvatar { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public string? PreviewContent { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public bool IsGroup { get; set; }
    public short TotalCount { get; set; }
    public short[] NotificationIds { get; set; } = [];
}

internal class NotificationFinalRow
{
    public NotificationRawDto Row { get; set; } = null!;
    public DateTimeOffset LastCreatedAt { get; set; }
}