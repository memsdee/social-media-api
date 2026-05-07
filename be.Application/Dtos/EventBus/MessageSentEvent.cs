using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class MessageSentEvent
{
    public short SequenceId { get; set; }
    public short ConversationSequeceId { get; set; }
    public Guid ConversationId { get; set; }
    public short SenderSequenceId { get; set; }
    public string SenderPublicId { get; set; } = null!;
    public string? Content { get; set; }
    public TypeMessageEnum Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? PreviewLastMess { get; set; }
}