using be.Application.Interfaces.BackgroundJob;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.BackgroundService;

public class TokenJob(WriteContext dbContext) : ITokenBgJob
{
    public async Task ClearExpiredTokensAsync(CancellationToken ctx)
    {
        await dbContext.Database.ExecuteSqlRawAsync(@"
                DELETE FROM ""auth"".""tokens""
                WHERE ""expires_at"" <= NOW()
            ", ctx);
    }
}