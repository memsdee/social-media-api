using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Documents;
using MediatR;

namespace be.Application.Features.Notification.GetListNoti;

public class GetListNotiHandler(
    IFormat helper,
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IEncryption encryption,
    INotificationReadRepository notificationReadRepository)
    : IRequestHandler<GetListNotiQuery, BaseResponse<GetListNotiResponse>>
{
    public async Task<BaseResponse<GetListNotiResponse>> Handle(
        GetListNotiQuery request,
        CancellationToken cancellationToken)
    {
        var myUserId = currentUserContext.UserId
                       ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        var userId = await userRepository.GetPrivateIdByPublicIdAsync(myUserId, cancellationToken)
                     ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        var cursor = request.Cursor is null
            ? null
            : JsonSerializer.Deserialize<CursorPayload<DateTimeOffset>>(encryption.Decrypt(request.Cursor));

        var fetchLimit = request.Limit * 5;

        var notificationsTask = notificationReadRepository
            .GetByReceiverAsync(userId, cursor?.Selector, cursor?.Id, fetchLimit, cancellationToken);

        var commentSenderCountsTask = notificationReadRepository
            .GetCommentUniqueSenderCountsAsync(userId, cancellationToken);

        var uniqueFollowSendersTask = notificationReadRepository
            .GetFollowUniqueSenderCountAsync(userId, cancellationToken);

        await Task.WhenAll(notificationsTask, commentSenderCountsTask, uniqueFollowSendersTask);

        var notifications = await notificationsTask;
        var commentSenderCounts = await commentSenderCountsTask;
        var uniqueFollowSenders = await uniqueFollowSendersTask;

        var senderIds = notifications.Select(x => x.SenderSequenceId).Distinct().ToArray();

        var senderMapTask = notificationReadRepository.GetUsersBySequenceIdsAsync(senderIds, cancellationToken);
        var postPublicIdMapTask = BuildPostPublicIdMapAsync(notifications, cancellationToken);
        var commentPublicIdMapTask = BuildCommentPublicIdMapAsync(notifications, cancellationToken);

        await Task.WhenAll(senderMapTask, postPublicIdMapTask, commentPublicIdMapTask);

        var senderMap = await senderMapTask;
        var postPublicIdMap = await postPublicIdMapTask;
        var commentPublicIdMap = await commentPublicIdMapTask;

        var baseRows = notifications.Select(noti =>
        {
            var sender = senderMap.GetValueOrDefault(noti.SenderSequenceId);

            var postId = noti.PostPublicId
                         ?? (noti.PostSequenceId.HasValue
                             ? postPublicIdMap.GetValueOrDefault(noti.PostSequenceId.Value)
                             : null);

            var commentId = noti.CmtPublicId
                            ?? (noti.CmtSequenceId.HasValue
                                ? commentPublicIdMap.GetValueOrDefault(noti.CmtSequenceId.Value)
                                : null);

            return new NotificationBaseRow
            {
                Id = noti.SequenceId,
                SenderId = noti.SenderSequenceId,
                SenderName = sender?.Name ?? string.Empty,
                SenderAvatar = sender?.Avatar,
                Target = noti.Target,
                Action = noti.Action,
                CreatedAt = noti.CreateAt,
                ReadAt = noti.ReadAt?.UtcDateTime,
                PostId = postId,
                CommentId = commentId,
                ThumbnailNoti = noti.ThumbnailNoti,
                PreviewContent = noti.PreviewContent,
                GroupKey = NotificationGrouper.BuildGroupKey(noti, commentSenderCounts, uniqueFollowSenders)
            };
        }).ToList();

        var groupedRows = NotificationGrouper.Group(baseRows, cursor, cursor?.Id);

        var hasNextPage = groupedRows.Count > request.Limit;
        var trimmedRows = groupedRows.Take(request.Limit).ToList();

        string? nextCursor = null;
        if (hasNextPage && trimmedRows.Count > 0)
        {
            var last = trimmedRows[^1];
            var payload = new CursorPayload<DateTimeOffset>(last.LastCreatedAt, last.Row.Id);
            nextCursor = encryption.Encrypt(JsonSerializer.Serialize(payload));
        }

        var mapper = new NotificationMapper(helper, encryption);
        var result = trimmedRows.Select(x => mapper.Map(x.Row)).ToList();

        return new BaseResponse<GetListNotiResponse>
        {
            Data = new GetListNotiResponse
            {
                Notifications = result,
                PageProfile = new PagiResult
                {
                    HasNextPage = hasNextPage,
                    NextCursor = nextCursor
                }
            }
        };
    }

    private async Task<Dictionary<short, Guid>> BuildPostPublicIdMapAsync(
        IReadOnlyCollection<NotificationDocument> notifications,
        CancellationToken ct)
    {
        var ids = notifications
            .Where(x => !x.PostPublicId.HasValue && x.PostSequenceId.HasValue)
            .Select(x => x.PostSequenceId!.Value)
            .Distinct().ToArray();

        return ids.Length == 0
            ? []
            : await notificationReadRepository.GetPostPublicIdsAsync(ids, ct);
    }

    private async Task<Dictionary<short, Guid>> BuildCommentPublicIdMapAsync(
        IReadOnlyCollection<NotificationDocument> notifications,
        CancellationToken ct)
    {
        var ids = notifications
            .Where(x => !x.CmtPublicId.HasValue && x.CmtSequenceId.HasValue)
            .Select(x => x.CmtSequenceId!.Value)
            .Distinct().ToArray();

        return ids.Length == 0
            ? []
            : await notificationReadRepository.GetCommentPublicIdsAsync(ids, ct);
    }
}