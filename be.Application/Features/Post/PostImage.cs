using be.Domain.Enums;

namespace be.Application.Features.Post;

public class PostImage
{
    public string Image { get; set; } = null!;
    public string? After { get; set; }
    public string? Before { get; set; }
    public ImageEnum Type { get; set; }
}