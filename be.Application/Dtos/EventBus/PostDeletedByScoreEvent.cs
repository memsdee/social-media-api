using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class PostDeletedByScoreEvent
{
    public List<Guid> ListIdPublicPost { get; set; } = [];
    public StatusPostEnum Status { get; set; }
}