using be.Application.Dtos.Queries.ThirdPartyIdentity;
using be.Domain.Entities;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IThirdPartyLoginsRepository
{
    Task<ThirdParty1Dto?> GetThirdParty1Async(string ggId, CancellationToken ctx);
    Task<bool> AnyLinkAsync(short privateAccountId, CancellationToken ctx);
    Task AddAsync(ThirdPartyLogin input, CancellationToken ctx);
    Task<bool> AnyThirdPartyMailAsync(short privateAccountId, string mail, CancellationToken ctx);
    Task<ThirdParty2Dto?> GetThirdParty2Async(short privateAccountId, CancellationToken ctx);
    Task DelAsync(short privateAccountId, CancellationToken ctx);

    Task<bool> AnyThirdPartyAsync(short privateAccountId, ThirdPartyLoginEnum thirdPartyLogin, string subjectId,
        CancellationToken ctx);
}