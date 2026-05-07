using System.Text.Json;
using be.Application.Dtos.Notification;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain.Enums;

namespace be.Application.Features.Notification.GetListNoti;

internal sealed class NotificationMapper(IFormat helper, IEncryption encryption)
{
    public NotificationDto Map(NotificationRawDto raw)
    {
        var thumbnailNoti = raw.ThumbnailNoti != Guid.Empty && raw.ThumbnailNoti.HasValue
            ? helper.FormatThumbnailNotiUrl(raw.ThumbnailNoti.Value)
            : null;

        var senderAvatar = helper.FormatImageUrl(raw.SenderAvatar, raw.SenderName);
        var encryptedIds = encryption.Encrypt(JsonSerializer.Serialize(raw.NotificationIds));

        return (raw.Target, raw.Action) switch
        {
            (NotiTargetEnum.post, NotiActionEnum.react) => new PostReactNotificationDto
            {
                PostId = raw.PostId!.Value,
                SenderName = raw.SenderName,
                SenderAvatar = senderAvatar,
                ThumbnailNoti = thumbnailNoti,
                PreviewContent = raw.PreviewContent ?? string.Empty,
                Target = NotiTargetEnum.post,
                Action = NotiActionEnum.react,
                IsGroup = raw.IsGroup,
                TotalCount = raw.TotalCount,
                CreatedAt = raw.CreatedAt,
                IsRead = raw.ReadAt != null,
                EncryptedIds = encryptedIds!
            },
            (NotiTargetEnum.post, NotiActionEnum.comment) => new PostCommentNotificationDto
            {
                PostId = raw.PostId!.Value,
                CommentId = raw.CommentId!.Value,
                SenderName = raw.SenderName,
                SenderAvatar = senderAvatar,
                ThumbnailNoti = thumbnailNoti,
                PreviewContent = raw.PreviewContent ?? string.Empty,
                Target = NotiTargetEnum.post,
                Action = NotiActionEnum.comment,
                IsGroup = raw.IsGroup,
                TotalCount = raw.TotalCount,
                CreatedAt = raw.CreatedAt,
                IsRead = raw.ReadAt != null,
                EncryptedIds = encryptedIds!
            },
            (NotiTargetEnum.user, NotiActionEnum.follow) => new UserFollowNotificationDto
            {
                SenderName = raw.SenderName,
                SenderAvatar = senderAvatar,
                Target = NotiTargetEnum.user,
                Action = NotiActionEnum.follow,
                IsGroup = raw.IsGroup,
                TotalCount = raw.TotalCount,
                CreatedAt = raw.CreatedAt,
                IsRead = raw.ReadAt != null,
                EncryptedIds = encryptedIds!
            },
            _ => throw new Exception($"Unhandled notification type: {raw.Target}/{raw.Action}")
        };
    }
}