using be.Application.Dtos.Pagination;
using be.Domain.Documents;
using be.Domain.Enums;

namespace be.Application.Features.Notification.GetListNoti;

internal static class NotificationGrouper
{
    public static List<NotificationFinalRow> Group(
        List<NotificationBaseRow> baseRows,
        CursorPayload<DateTimeOffset>? cursor,
        short? cursorId)
    {
        var grouped = baseRows
            .GroupBy(x => x.GroupKey)
            .Select(g =>
            {
                var anchorId = g.Max(x => x.Id);
                var anchor = g.First(x => x.Id == anchorId);
                var senderCount = g.Select(x => x.SenderId).Distinct().Count();

                return new NotificationFinalRow
                {
                    Row = new NotificationRawDto
                    {
                        Id = anchor.Id,
                        CreatedAt = anchor.CreatedAt,
                        ReadAt = anchor.ReadAt,
                        PostId = anchor.PostId,
                        CommentId = anchor.CommentId,
                        SenderName = anchor.SenderName,
                        SenderAvatar = anchor.SenderAvatar,
                        ThumbnailNoti = anchor.ThumbnailNoti,
                        PreviewContent = anchor.PreviewContent,
                        Target = anchor.Target,
                        Action = anchor.Action,
                        IsGroup = senderCount > 1,
                        TotalCount = (short)senderCount,
                        NotificationIds = g.Select(x => x.Id).OrderByDescending(x => x).ToArray()
                    },
                    LastCreatedAt = anchor.CreatedAt
                };
            })
            .ToList();

        if (cursor is not null && cursorId.HasValue)
            grouped = grouped
                .Where(x =>
                    x.LastCreatedAt < cursor.Selector ||
                    (x.LastCreatedAt == cursor.Selector && x.Row.Id < cursorId.Value))
                .ToList();

        return grouped
            .OrderByDescending(x => x.LastCreatedAt)
            .ThenByDescending(x => x.Row.Id)
            .ToList();
    }

    public static string BuildGroupKey(
        NotificationDocument noti,
        IReadOnlyDictionary<short, int> commentSenderCounts,
        int uniqueFollowSenders)
    {
        if (noti.Action == NotiActionEnum.react && noti.Target == NotiTargetEnum.post)
            return noti.PostSequenceId.HasValue
                ? $"react_{noti.PostSequenceId.Value}"
                : $"single_{noti.SequenceId}";

        if (noti.Action == NotiActionEnum.comment &&
            noti.Target == NotiTargetEnum.post &&
            noti.PostSequenceId.HasValue &&
            commentSenderCounts.GetValueOrDefault(noti.PostSequenceId.Value) > 1)
            return $"comment_{noti.PostSequenceId.Value}";

        if (noti.Action == NotiActionEnum.follow &&
            noti.Target == NotiTargetEnum.user &&
            uniqueFollowSenders > 1)
            return "follow_group";

        return $"single_{noti.SequenceId}";
    }
}