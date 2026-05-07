namespace be.Application.Dtos.EventBus;

public class UpdateAvatarEvent
{
    public short PrivateUserId { get; set; }
    public Guid Avatar { get; set; }
}