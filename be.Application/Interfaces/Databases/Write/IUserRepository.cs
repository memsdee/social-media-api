using be.Application.Dtos.Queries.User;
using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IUserRepository
{
    Task<short?> GetPrivateIdByPublicIdAsync(string publicId, CancellationToken ct);
    Task<User1Dto?> GetUserByPublicIdAsync(string publicUserId, CancellationToken ct);
    Task UpdateNameAsync(string newName, short pritaveUserId, CancellationToken ct);
    Task UpdateUserIdAsync(short pritaveUserId, string newUserId, CancellationToken ct);
    Task UpdateBioAsync(short pritaveUserId, string newBio, CancellationToken ct);
    Task AddAsync(User dto, CancellationToken ct);
    Task<User2Dto?> GetUser2Async(string publicUserId, CancellationToken ct);
    Task<bool> ExistsAsync(string publicUserId, CancellationToken ct);
    Task<User5Dto?> GetUser5Async(string publicUserId, CancellationToken ct);
    Task<int> DeleteAvatarAsync(short pritaveUserId, Guid avatar, CancellationToken ct);
    Task<User6Dto?> GetUser6Async(string publicUserId, CancellationToken ct);
    Task<ReportUserInfoDto?> GetReportUserInfoAsync(string publicUserId, CancellationToken ct);
    Task<Dictionary<short, string>> GetPublicIdsByPrivateIdsAsync(IEnumerable<short> privateIds, CancellationToken ct);
}