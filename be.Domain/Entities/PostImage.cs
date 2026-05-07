using be.Domain.Enums;

namespace be.Domain.Entities;

public class PostImage
{
    public short Id { get; set; }
    public Guid Image { get; set; }
    public ImageEnum Type { get; set; }
    public short? GroupId { get; set; }
    public short PostId { get; set; }

    public virtual Post PostNavi { get; set; } = null!;
}