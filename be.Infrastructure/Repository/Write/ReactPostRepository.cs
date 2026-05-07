using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class ReactPostRepository(WriteContext dbContext) : IReactPostRepository
{
    public async Task AddAsync(ReactPost reactPost, CancellationToken ct)
    {
        await dbContext.ReactPosts.AddAsync(reactPost, ct);
    }

    public void Delete(ReactPost reactPost)
    {
        dbContext.ReactPosts.Remove(reactPost);
    }

    public async Task<ReactPost?> GetByUserAndPostAsync(short userId, short postId, CancellationToken ct)
    {
        return await dbContext.ReactPosts.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId, ct);
    }

    public async Task<Dictionary<ReactEnum, short>> GetTotalReactsAsync(short postId, CancellationToken ct)
    {
        return await dbContext.ReactPosts
            .Where(x => x.PostId == postId)
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Count = (short)g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, ct);
    }
}