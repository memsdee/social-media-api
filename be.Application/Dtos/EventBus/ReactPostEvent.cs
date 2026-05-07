using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class ReactPostEvent
{
    public short SequenceId { get; set; }
    public short PostSequenceId { get; set; }
    public Guid PostIdPublic { get; set; }
    public short UserSequenceId { get; set; }
    public string UserPublicId { get; set; } = null!;
    public ReactEnum Type { get; set; }
    public bool IsDelete { get; set; }
}