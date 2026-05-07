using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class NotiReactPostRepository(WriteContext dbContext) : INotiReactPostRepository
{
    public async Task AddAsync(NotiReactPost notiReactPost, CancellationToken ct)
    {
        await dbContext.NotiReactPosts.AddAsync(notiReactPost, ct);
    }

    public async Task<List<short>> GetNotiIdsByPostIdAsync(short postId, CancellationToken ct)
    {
        return await dbContext.NotiReactPosts.Where(x => x.PostId == postId).Select(x => x.NotiId).ToListAsync(ct);
    }

    public async Task<NotiReactPost?> GetByIdAsync(short notiId, CancellationToken ct)
    {
        return await dbContext.NotiReactPosts.FirstOrDefaultAsync(x => x.NotiId == notiId, ct);
    }
}