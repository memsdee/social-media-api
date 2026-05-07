using be.Application.Dtos.Queries.User;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class UserRepository(WriteContext dbContext) : IUserRepository
{
    public async Task<short?> GetPrivateIdByPublicIdAsync(string publicId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking()
            .Where(u => u.UserId == publicId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<User1Dto?> GetUserByPublicIdAsync(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users
            .Where(u => u.UserId == publicUserId)
            .Select(x => new User1Dto
            {
                Id = x.Id,
                Bio = x.Bio,
                Name = x.Name,
                PublicUserId = x.UserId,
                Role = x.AccountNavi.Role
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpdateNameAsync(string newName, short pritaveUserId, CancellationToken ct)
    {
        await dbContext.Users.Where(x => x.Id == pritaveUserId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Name, newName), ct);
    }

    public async Task UpdateUserIdAsync(short pritaveUserId, string newUserId, CancellationToken ct)
    {
        await dbContext.Users.Where(x => x.Id == pritaveUserId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.UserId, newUserId), ct);
    }

    public async Task UpdateBioAsync(short pritaveUserId, string newBio, CancellationToken ct)
    {
        await dbContext.Users.Where(x => x.Id == pritaveUserId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Bio, newBio), ct);
    }

    public async Task AddAsync(User dto, CancellationToken ct)
    {
        await dbContext.Users.AddAsync(dto, ct);
    }

    public async Task<User2Dto?> GetUser2Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking().Where(x => x.UserId == publicUserId)
            .Select(x => new User2Dto
            {
                SequenceId = x.Id,
                PublicUserId = x.UserId,
                Name = x.Name,
                Avatar = x.Avatar,
                IsDeleteAccount = x.AccountNavi.Status != StatusAccountEnum.active
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking().AnyAsync(x => x.UserId == publicUserId, ct);
    }

    public async Task<User5Dto?> GetUser5Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking().Where(x => x.UserId == publicUserId)
            .Select(x => new User5Dto
            {
                Avatar = x.Avatar,
                PublicUserId = x.UserId,
                PrivateUserId = x.Id
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<int> DeleteAvatarAsync(short pritaveUserId, Guid avatar, CancellationToken ct)
    {
        return await dbContext.Users.Where(x => x.Id == pritaveUserId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Avatar, avatar), ct);
    }

    public async Task<User6Dto?> GetUser6Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking().Where(x => x.UserId == publicUserId)
            .Select(x => new User6Dto
            {
                Avatar = x.Avatar,
                PublicUserId = x.UserId,
                PrivateUserId = x.Id,
                Name = x.Name
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<ReportUserInfoDto?> GetReportUserInfoAsync(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Users.Where(x => x.UserId == publicUserId)
            .Select(x => new ReportUserInfoDto
            {
                Id = x.Id,
                PublicId = x.UserId,
                Name = x.Name,
                Avatar = x.Avatar,
                Email = x.AccountNavi.Mail ?? string.Empty
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<short, string>> GetPublicIdsByPrivateIdsAsync(IEnumerable<short> privateIds,
        CancellationToken ct)
    {
        var ids = privateIds.Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        return await dbContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.UserId, ct);
    }
}