using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class NotiReactPostEvent
{
    public short SequenceId { get; set; }
    public short ReceiveSequenceId { get; set; }
    public short SenderSequenceId { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public short PostSequenceId { get; set; }
    public Guid PostIdPublic { get; set; }
    public ReactEnum Type { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public string? PreviewContent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDelete { get; set; }
}