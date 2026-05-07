using be.Application.Dtos.Shared;
using be.Domain.Enums;

namespace be.Application.Interfaces.Security;

public interface ITokenGenerator
{
    Task<BaseResponse<string>> CreateAccessTokensAsync(string userId, RoleEnum role);
    Task<BaseResponse<string>> CreateRefreshTokenAsync(short idAccount, CancellationToken cancellation);
}