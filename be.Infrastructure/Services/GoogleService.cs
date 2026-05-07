using System.Net.Http.Json;
using be.Application.Dtos.OAuth;
using be.Application.Interfaces.External;
using be.Infrastructure.Common.Appsetting;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services;

public class GoogleService(HttpClient httpClient, IOptions<OauthSettings> options) : IGoogle
{
    private readonly GoogleSettings _googleSettings = options.Value.Google;

    public async Task<GoogleDto> GetTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "token");
        var parameters = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleSettings.ClientId },
            { "client_secret", _googleSettings.ClientSecret },
            { "redirect_uri", _googleSettings.RedirectUri },
            { "grant_type", "authorization_code" }
        };

        request.Content = new FormUrlEncodedContent(parameters);

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<GoogleDto>(cancellationToken) ?? throw new Exception();

        throw new Exception();
    }

    public async Task<GoogleUserInfoDto> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_googleSettings.ClientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfoDto
            {
                ExternalId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                Picture = payload.Picture,
                GivenName = payload.GivenName,
                FamilyName = payload.FamilyName
            };
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google Token.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Error during Google token validation.", ex);
        }
    }
}