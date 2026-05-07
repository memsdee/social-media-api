namespace be.Application.Dtos.EventBus;

public class MarkReadMessEvent
{
    public short ConversationSequenceId { get; set; }
    public short UserSequenceId { get; set; }
    public string UserPublicId { get; set; } = null!;
    public DateTimeOffset ReadAt { get; set; }
}