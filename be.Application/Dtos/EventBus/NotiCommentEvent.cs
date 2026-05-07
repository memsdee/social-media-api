using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class NotiCommentEvent
{
    public short SequenceId { get; set; }
    public short ReceiveSequenceId { get; set; }
    public short SenderSequenceId { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public short PostSequenceId { get; set; }
    public Guid PostPublicId { get; set; }
    public short CmtSequenceId { get; set; }
    public Guid CmtPublicId { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public string PreviewContent { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}