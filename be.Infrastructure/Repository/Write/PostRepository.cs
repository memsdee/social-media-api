using be.Application.Dtos.Queries.Posts;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class PostRepository(WriteContext dbContext) : IPostRepository
{
    public async Task AddAsync(Post post, CancellationToken ct)
    {
        await dbContext.Posts.AddAsync(post, ct);
    }

    public async Task<Post?> GetPostByIdPublicAsync(Guid idPublic, CancellationToken ct)
    {
        return await dbContext.Posts.FirstOrDefaultAsync(x => x.IdPublic == idPublic, ct);
    }

    public async Task<Post?> GetPostWithNaviByIdPublicAsync(Guid idPublic, CancellationToken ct)
    {
        return await dbContext.Posts
            .Include(x => x.UserNavi)
            .Include(x => x.PostImageNavi)
            .FirstOrDefaultAsync(x => x.IdPublic == idPublic, ct);
    }

    public async Task<PostForCommentDto?> GetPostForCommentAsync(Guid idPublic, CancellationToken ct)
    {
        return await dbContext.Posts.Where(x => x.IdPublic == idPublic)
            .Select(x => new PostForCommentDto
            {
                Id = x.Id,
                UserId = x.UserId,
                PostAuthor = x.UserNavi.UserId,
                Image = x.PostImageNavi.Select(a => a.Image).FirstOrDefault(),
                Content = x.Content ?? string.Empty
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<PostForReactDto?> GetPostForReactAsync(Guid idPublic, short currentUserId, CancellationToken ct)
    {
        return await dbContext.Posts.Where(x => x.IdPublic == idPublic)
            .Select(x => new PostForReactDto
            {
                Id = x.Id,
                UserId = x.UserId,
                PostAuthorPublicId = x.UserNavi.UserId,
                Thumbnail = x.PostImageNavi.Select(a => a.Image).FirstOrDefault(),
                Content = x.Content,
                AuthorReact = x.ReacPostNavi.Where(r => r.UserId == currentUserId).Select(r => (ReactEnum?)r.Type)
                    .FirstOrDefault()
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<Post?> GetAdminPostByIdAsync(int id, CancellationToken ct)
    {
        return await dbContext.Posts
            .IgnoreQueryFilters()
            .Include(x => x.UserNavi)
            .Include(x => x.PostImageNavi)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<int> UpdateStatusAsync(Guid idPublic, short userId, StatusPostEnum status, CancellationToken ct)
    {
        return await dbContext.Posts.Where(x => x.IdPublic == idPublic && x.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, status), ct);
    }

    public async Task AddAdminDelPostLogAsync(AdminDelPostLog log, CancellationToken ct)
    {
        await dbContext.AdminDeletePostLogs.AddAsync(log, ct);
    }

    public async Task<int> AdminDeletePostAsync(int postId, int scoreToAdd, CancellationToken ct)
    {
        return await dbContext.Posts.Where(x => x.Id == postId).ExecuteUpdateAsync(c => c
            .SetProperty(v => v.Status, StatusPostEnum.deleted)
            .SetProperty(v => v.ScoreReport, v => v.ScoreReport + scoreToAdd), ct);
    }

    public async Task<int> UpdateScoreAsync(int postId, int newScore, CancellationToken ct)
    {
        return await dbContext.Posts.Where(x => x.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ScoreReport, newScore), ct);
    }
}