using be.Application.Dtos.Queries.Account;
using be.Domain.Entities;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IAccountRepository
{
    Task<Account1Dto?> GetAccount1Async(string publicId, CancellationToken ct);
    Task ChangeMailAsync(short privateAccountId, string newMail, CancellationToken ct);
    Task ChangePasswordAsync(short privateAccountId, string newPassword, CancellationToken ct);
    Task<int> ChangePasswordByMailAsync(string mail, string newPassword, CancellationToken ct);
    Task<Account2Dto?> GetAccount2Async(string email, CancellationToken ct);
    Task<Account3Dto?> GetAccount3Async(string email, CancellationToken ct);
    Task AddAsync(Account dto, CancellationToken ct);
    Task<Account4Dto?> GetAccount4Async(short privateAccountId, CancellationToken ct);
    Task<bool> AnyMailAsync(string mail, CancellationToken ct);
    Task<Account5Dto> GetAccount5Async(string publicUserId, CancellationToken ct);
    Task<bool> AnyAccountAsync(string publicUserId, string mail, CancellationToken ct);
    Task UpdateThirdPartyAsync(short privateAccountId, CancellationToken ct);
    Task<Account6Dto?> GetAccount6Async(string publicUserId, CancellationToken ct);
    Task<bool> AnyAccountMailAsync(short privateAccountId, string mail, CancellationToken ct);
    Task<int> UpdateAccountAsync(short privateAccountId, string mail, string passHash, CancellationToken ct);
    Task<Account7Dto> GetAccount7Async(string publicUserId, CancellationToken ct);
    Task UpdateThirdPartyAsync(short privateAccountId, bool value, CancellationToken ct);
    Task<Account8Dto?> GetAccount8Async(string publicUserId, CancellationToken ct);
    Task UpdateStatusAsync(short privateAccountId, StatusAccountEnum status, CancellationToken ct);
    Task<Account9Dto?> GetAccount9Async(string publicUserId, CancellationToken ct);
    Task<int> BanAccountAsync(short privateAccountId, CancellationToken ct);
    Task AddAdminDelAccountLogAsync(AdminDelAccountLog log, CancellationToken ct);
    Task AddScoreAsync(short privateAccountId, int scoreToAdd, CancellationToken ct);
    Task UpdateScoreAsync(short privateAccountId, int newScore, CancellationToken ct);
}