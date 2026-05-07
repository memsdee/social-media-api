namespace be.Application.Dtos.Queries.User;

public class ReportUserInfoDto
{
    public short Id { get; set; }
    public string PublicId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public string Email { get; set; } = null!;
}