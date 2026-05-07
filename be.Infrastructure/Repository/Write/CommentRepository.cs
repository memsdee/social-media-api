using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class CommentRepository(WriteContext dbContext) : ICommentRepository
{
    public async Task AddAsync(Comment comment, CancellationToken ct)
    {
        await dbContext.Comments.AddAsync(comment, ct);
    }

    public async Task<int> CountByPostIdAsync(short postId, CancellationToken ct)
    {
        return await dbContext.Comments.CountAsync(x => x.PostId == postId, ct);
    }

    public async Task<Comment?> GetByIdPublicAsync(Guid idPublic, CancellationToken ct)
    {
        return await dbContext.Comments.FirstOrDefaultAsync(x => x.IdPublic == idPublic, ct);
    }

    public void Delete(Comment comment)
    {
        dbContext.Comments.Remove(comment);
    }

    public async Task<List<short>> GetNotiIdsByCommentIdAsync(short commentId, CancellationToken ct)
    {
        return await dbContext.NotiCmts.Where(x => x.CmtId == commentId).Select(x => x.NotiId).ToListAsync(ct);
    }
}