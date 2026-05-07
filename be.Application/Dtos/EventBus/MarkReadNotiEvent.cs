namespace be.Application.Dtos.EventBus;

public class MarkReadNotiEvent
{
    public short[] NotificationIds { get; set; } = null!;
    public short ReceiverSequenceId { get; set; }
    public DateTimeOffset ReadAt { get; set; }
}