using be.Domain.Enums;

namespace be.Application.Dtos.Queries.User;

public class User1Dto
{
    public string? Bio { get; set; }
    public short Id { get; set; }
    public string Name { get; set; } = null!;
    public string PublicUserId { get; set; } = null!;
    public RoleEnum Role { get; set; }
}