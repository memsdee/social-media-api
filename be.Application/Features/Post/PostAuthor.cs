namespace be.Application.Features.Post;

public class PostAuthor
{
    public string? UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string UserAvatar { get; set; } = null!;
    public bool IsFollow { get; set; }
}