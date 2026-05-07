namespace be.Application.Dtos.EventBus;

public class UnfollowEvent
{
    public short FollowerSequenceId { get; set; }
    public short FolloweeSequenceId { get; set; }
}