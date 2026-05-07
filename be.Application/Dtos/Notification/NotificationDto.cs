using System.Text.Json.Serialization;
using be.Domain.Enums;

namespace be.Application.Dtos.Notification;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PostReactNotificationDto), "post_react")]
[JsonDerivedType(typeof(PostCommentNotificationDto), "post_comment")]
[JsonDerivedType(typeof(UserFollowNotificationDto), "user_follow")]
public abstract class NotificationDto
{
    public string SenderName { get; set; } = null!;
    public string SenderAvatar { get; set; } = null!;
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string EncryptedIds { get; set; } = null!;
}