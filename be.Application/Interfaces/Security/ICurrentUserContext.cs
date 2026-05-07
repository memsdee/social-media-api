namespace be.Application.Interfaces.Security;

public interface ICurrentUserContext
{
    string? UserId { get; }
    string? RefreshToken { get; }
}