using be.Application.Dtos.Queries.Account;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class AccountRepository(WriteContext dbContext) : IAccountRepository
{
    public async Task<Account1Dto?> GetAccount1Async(string publicId, CancellationToken ct)
    {
        return await dbContext.Users.AsNoTracking().Where(x => x.UserId == publicId)
            .Select(x => new Account1Dto
            {
                AccountId = x.AccountId,
                Mail = x.AccountNavi.Mail,
                Pass = x.AccountNavi.Pass
            }).FirstOrDefaultAsync(ct);
    }

    public async Task ChangeMailAsync(short privateAccountId, string newMail, CancellationToken ct)
    {
        await dbContext.Accounts.AsNoTracking().Where(x => x.Id == privateAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Mail, newMail), ct);
    }

    public async Task ChangePasswordAsync(short privateAccountId, string newPassword, CancellationToken ct)
    {
        await dbContext.Accounts.AsNoTracking().Where(x => x.Id == privateAccountId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Pass, newPassword), ct);
    }

    public async Task<int> ChangePasswordByMailAsync(string mail, string newPassword, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.Mail == mail)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Pass, newPassword), ct);
    }

    public async Task<Account2Dto?> GetAccount2Async(string email, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.Mail == email)
            .Select(x => new Account2Dto
            {
                Pass = x.Pass,
                Role = x.Role,
                PrivateAccountId = x.Id,
                PublicUserId = x.UserNavi.UserId,
                IsThirdParty = x.IsThirdParty
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<Account3Dto?> GetAccount3Async(string email, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.Mail == email)
            .Select(x => new Account3Dto
            {
                Pass = x.Pass,
                PrivateAccountId = x.Id,
                PublicUserId = x.UserNavi.UserId,
                IsThirdParty = x.IsThirdParty
            }).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Account dto, CancellationToken ct)
    {
        await dbContext.Accounts.AddAsync(dto, ct);
    }

    public async Task<Account4Dto?> GetAccount4Async(short privateAccountId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.Id == privateAccountId)
            .Select(x => new Account4Dto
            {
                Role = x.Role,
                PrivateAccountId = x.Id,
                PublicUserId = x.UserNavi.UserId
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> AnyMailAsync(string mail, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().AnyAsync(x => x.Mail == mail, ct);
    }

    public async Task<Account5Dto> GetAccount5Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.UserNavi.UserId == publicUserId)
                   .Select(x => new Account5Dto
                   {
                       HasMail = string.IsNullOrEmpty(x.Mail),
                       HasGoogle = x.IsThirdParty
                   }).FirstOrDefaultAsync(ct)
               ?? throw new Exception("Tài khoản không tồn tại!");
    }

    public async Task<bool> AnyAccountAsync(string publicUserId, string mail, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().AnyAsync(a =>
            a.UserNavi.UserId != publicUserId &&
            (
                a.Mail == mail ||
                a.ThirdPartyLoginsNavi.Any(t => t.Mail == mail)
            ), ct);
    }

    public async Task UpdateThirdPartyAsync(short privateAccountId, CancellationToken ct)
    {
        await dbContext.Accounts.Where(a => a.Id == privateAccountId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsThirdParty, true), ct);
    }

    public async Task<Account6Dto?> GetAccount6Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.UserNavi.UserId == publicUserId)
            .Select(x => new Account6Dto
            {
                PrivateAccountId = x.Id,
                Pass = x.Pass
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> AnyAccountMailAsync(short privateAccountId, string mail, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().AnyAsync(a =>
            a.Id != privateAccountId && a.Mail == mail, ct);
    }

    public async Task<int> UpdateAccountAsync(short privateAccountId, string mail, string passHash,
        CancellationToken ct)
    {
        return await dbContext.Accounts.Where(a => a.Id == privateAccountId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Mail, mail)
                .SetProperty(a => a.Pass, passHash), ct);
    }

    public async Task<Account7Dto> GetAccount7Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.UserNavi.UserId == publicUserId)
                   .Select(x => new Account7Dto
                   {
                       PrivateAccountId = x.Id,
                       HasPass = !string.IsNullOrEmpty(x.Pass)
                   }).FirstOrDefaultAsync(ct)
               ?? throw new Exception("Tài khoản không tồn tại!");
    }

    public async Task UpdateThirdPartyAsync(short privateAccountId, bool value, CancellationToken ct)
    {
        await dbContext.Accounts.Where(a => a.Id == privateAccountId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsThirdParty, value), ct);
    }

    public async Task<Account8Dto?> GetAccount8Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.UserNavi.UserId == publicUserId)
            .Select(x => new Account8Dto
            {
                IsDeleted = x.Status == StatusAccountEnum.deleted,
                PrivateAccountId = x.Id,
                Mail = x.Mail,
                Pass = x.Pass,
                IsThirdParty = x.IsThirdParty
            }).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateStatusAsync(short privateAccountId, StatusAccountEnum status, CancellationToken ct)
    {
        await dbContext.Accounts.Where(a => a.Id == privateAccountId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, status), ct);
    }

    public async Task<Account9Dto?> GetAccount9Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Accounts.AsNoTracking().Where(x => x.UserNavi.UserId == publicUserId)
            .Select(x => new Account9Dto
            {
                IsDeleted = x.Status == StatusAccountEnum.deleted,
                IsThirdParty = x.IsThirdParty,
                PrivateAccountId = x.Id
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<int> BanAccountAsync(short privateAccountId, CancellationToken ct)
    {
        return await dbContext.Accounts.Where(x =>
                x.Id == privateAccountId && x.Status != StatusAccountEnum.banned &&
                x.Status != StatusAccountEnum.deleted)
            .ExecuteUpdateAsync(c => c.SetProperty(v => v.Status, StatusAccountEnum.banned), ct);
    }

    public async Task AddAdminDelAccountLogAsync(AdminDelAccountLog log, CancellationToken ct)
    {
        await dbContext.AdminDelAccountLogs.AddAsync(log, ct);
    }

    public async Task AddScoreAsync(short privateAccountId, int scoreToAdd, CancellationToken ct)
    {
        await dbContext.Accounts.Where(x => x.Id == privateAccountId)
            .ExecuteUpdateAsync(c => c.SetProperty(v => v.Score, v => v.Score + scoreToAdd), ct);
    }

    public async Task UpdateScoreAsync(short privateAccountId, int newScore, CancellationToken ct)
    {
        await dbContext.Accounts.Where(x => x.Id == privateAccountId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Score, newScore), ct);
    }
}