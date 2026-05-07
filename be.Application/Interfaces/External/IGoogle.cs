using be.Application.Dtos.OAuth;

namespace be.Application.Interfaces.External;

public interface IGoogle
{
    Task<GoogleDto> GetTokenAsync(string code, CancellationToken cancellationToken);
    Task<GoogleUserInfoDto> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken);
}