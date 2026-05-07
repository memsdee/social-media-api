using be.Application.Dtos.Queries.ThirdPartyIdentity;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class ThirdPartyLoginsRepository(WriteContext dbContext) : IThirdPartyLoginsRepository
{
    public async Task<ThirdParty1Dto?> GetThirdParty1Async(string ggId, CancellationToken ctx)
    {
        return await dbContext.ThirdPartyLogins
            .Where(t => t.Provider == ThirdPartyLoginEnum.google && t.ProviderId == ggId)
            .Select(t => new ThirdParty1Dto
            {
                PublicUserId = t.AccountNavi.UserNavi.UserId,
                PrivateAccountId = t.AccountId
            }).FirstOrDefaultAsync(ctx);
    }

    public async Task<bool> AnyLinkAsync(short privateAccountId, CancellationToken ctx)
    {
        return await dbContext.ThirdPartyLogins.AsNoTracking().Where(t =>
                t.AccountId == privateAccountId && t.Provider == ThirdPartyLoginEnum.google)
            .AnyAsync(ctx);
    }

    public async Task AddAsync(ThirdPartyLogin input, CancellationToken ctx)
    {
        await dbContext.ThirdPartyLogins.AddAsync(input, ctx);
    }

    public async Task<bool> AnyThirdPartyMailAsync(short privateAccountId, string mail, CancellationToken ctx)
    {
        return await dbContext.ThirdPartyLogins.AsNoTracking()
            .Where(t => t.AccountId != privateAccountId && t.Mail == mail)
            .AnyAsync(ctx);
    }

    public async Task<ThirdParty2Dto?> GetThirdParty2Async(short privateAccountId, CancellationToken ctx)
    {
        return await dbContext.ThirdPartyLogins.AsNoTracking()
            .Where(t => t.AccountId == privateAccountId)
            .GroupBy(_ => 1)
            .Select(g => new ThirdParty2Dto
            {
                HasGoogle = g.Any(x => x.Provider == ThirdPartyLoginEnum.google),
                HasOtherThirdParty = g.Any(x => x.Provider != ThirdPartyLoginEnum.google)
            }).FirstOrDefaultAsync(ctx);
    }

    public async Task DelAsync(short privateAccountId, CancellationToken ctx)
    {
        await dbContext.ThirdPartyLogins
            .Where(t => t.AccountId == privateAccountId && t.Provider == ThirdPartyLoginEnum.google)
            .ExecuteDeleteAsync(ctx);
    }

    public async Task<bool> AnyThirdPartyAsync(short privateAccountId, ThirdPartyLoginEnum thirdPartyLogin,
        string subjectId, CancellationToken ctx)
    {
        return await dbContext.ThirdPartyLogins.AsNoTracking()
            .Where(t => t.Provider == thirdPartyLogin && t.ProviderId == subjectId && t.AccountId == privateAccountId)
            .AnyAsync(ctx);
    }
}