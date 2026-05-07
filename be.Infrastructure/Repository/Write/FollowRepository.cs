using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class FollowRepository(WriteContext dbContext) : IFollowRepository
{
    public async Task AddAsync(Follow input, CancellationToken ct)
    {
        await dbContext.Follows.AddAsync(input, ct);
    }

    public async Task<int> UnFollowAsync(short privateFollowerId, short privateFolloweedId, CancellationToken ct)
    {
        return await dbContext.Follows
            .Where(f => f.FollowerId == privateFollowerId && f.FolloweeId == privateFolloweedId)
            .ExecuteDeleteAsync(ct);
    }
}