namespace be.Application.Dtos.OAuth;

public class GoogleUserInfoDto
{
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Picture { get; set; }
    public string ExternalId { get; set; } = null!;
    public bool IsEmailVerified { get; set; }
    public string Subject { get; set; } = null!;
}