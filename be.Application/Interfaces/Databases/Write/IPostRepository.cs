using be.Application.Dtos.Queries.Posts;
using be.Domain.Entities;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IPostRepository
{
    Task AddAsync(Post post, CancellationToken ct);
    Task<Post?> GetPostByIdPublicAsync(Guid idPublic, CancellationToken ct);
    Task<Post?> GetPostWithNaviByIdPublicAsync(Guid idPublic, CancellationToken ct);
    Task<PostForCommentDto?> GetPostForCommentAsync(Guid idPublic, CancellationToken ct);
    Task<PostForReactDto?> GetPostForReactAsync(Guid idPublic, short currentUserId, CancellationToken ct);
    Task<Post?> GetAdminPostByIdAsync(int id, CancellationToken ct);
    Task<int> UpdateStatusAsync(Guid idPublic, short userId, StatusPostEnum status, CancellationToken ct);
    Task AddAdminDelPostLogAsync(AdminDelPostLog log, CancellationToken ct);
    Task<int> AdminDeletePostAsync(int postId, int scoreToAdd, CancellationToken ct);
    Task<int> UpdateScoreAsync(int postId, int newScore, CancellationToken ct);
}