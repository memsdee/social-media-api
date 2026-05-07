using be.Domain.Enums;

namespace be.Application.Features.Post.Post.AddPost;

public class ImageItem
{
    public Guid Image { get; set; }
    public ImageEnum Type { get; set; }
    public short? GroupId { get; set; }
}