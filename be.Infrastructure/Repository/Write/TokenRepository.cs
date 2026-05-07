using be.Application.Dtos.Queries.Token;
using be.Application.Interfaces.Databases.Write;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class TokenRepository(WriteContext dbContext) : ITokenRepository
{
    public async Task DelRefreshTokenAsync(string hashRefreshToken, CancellationToken ct)
    {
        await dbContext.Tokens.Where(t => t.RefreshToken == hashRefreshToken).ExecuteDeleteAsync(ct);
    }

    public async Task<Token1Dto?> GetToken1Async(string hashRefreshToken, CancellationToken ct)
    {
        return await dbContext.Tokens.Where(x => x.RefreshToken == hashRefreshToken)
            .Select(x => new Token1Dto
            {
                ExpiresAt = x.ExpiresAt,
                PrivateAccountId = x.AccountId
            }).FirstOrDefaultAsync(ct);
    }

    public async Task DelAsync(string hashRefreshToken, CancellationToken ct)
    {
        await dbContext.Tokens.Where(t => t.RefreshToken == hashRefreshToken).ExecuteDeleteAsync(ct);
    }

    public async Task DelByPrivateAccountIdAsync(short privateAccountId, CancellationToken ct)
    {
        await dbContext.Tokens.Where(t => t.AccountId == privateAccountId).ExecuteDeleteAsync(ct);
    }
}