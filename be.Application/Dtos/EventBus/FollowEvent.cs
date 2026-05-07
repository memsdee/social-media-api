namespace be.Application.Dtos.EventBus;

public class FollowEvent
{
    public short Sequence { get; set; }
    public short FollowerSequenceId { get; set; }
    public string FollowerIdPublic { get; set; } = null!;
    public string FollowerName { get; set; } = null!;
    public Guid? FollowerAvatar { get; set; }
    public bool FollowerIsDeleteAccount { get; set; }
    public short FolloweeSequenceId { get; set; }
    public string FolloweeIdPublic { get; set; } = null!;
    public string FolloweeName { get; set; } = null!;
    public Guid? FolloweeAvatar { get; set; }
    public bool FolloweeIsDeleteAccount { get; set; }
    public DateTimeOffset CreateAt { get; set; }
}