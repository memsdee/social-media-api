using System.Net;
using be.Application.Interfaces.Security;
using be.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace be.TestE2E;

public abstract class BaseE2ETest : IClassFixture<TestWebApplicationFactory>
{
    private readonly CookieContainer _cookieContainer = new();
    protected readonly HttpClient Client;
    protected readonly TestWebApplicationFactory Factory;

    protected BaseE2ETest(TestWebApplicationFactory factory)
    {
        Factory = factory;

        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }

    protected async Task AuthenticateAsync(string userId = "testuser", RoleEnum role = RoleEnum.user)
    {
        using var scope = Factory.Services.CreateScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<ITokenGenerator>();
        var response = await tokenGenerator.CreateAccessTokensAsync(userId, role);
        var token = response.Data!;

        Client.DefaultRequestHeaders.Remove("Cookie");
        Client.DefaultRequestHeaders.Add("Cookie", $"accessToken={token}");
    }
}