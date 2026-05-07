using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class NotiFollowEvent
{
    public short SequenceId { get; set; }
    public string SenderPublicUserId { get; set; } = null!;
    public short SenderPrivateUserId { get; set; }
    public string ReciverPublicUserId { get; set; } = null!;
    public short ReciverPrivateUserId { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}