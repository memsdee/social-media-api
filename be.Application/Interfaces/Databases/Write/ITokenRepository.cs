using be.Application.Dtos.Queries.Token;

namespace be.Application.Interfaces.Databases.Write;

public interface ITokenRepository
{
    Task DelRefreshTokenAsync(string hashRefreshToken, CancellationToken ct);
    Task<Token1Dto?> GetToken1Async(string hashRefreshToken, CancellationToken ct);
    Task DelAsync(string hashRefreshToken, CancellationToken ct);
    Task DelByPrivateAccountIdAsync(short privateAccountId, CancellationToken ct);
}