using System.Net;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.Change.ChangePassword;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace be.TestE2E.Account;

public class ChangePasswordTests : BaseE2ETest
{
    public ChangePasswordTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("", "newpassword123", "OldPass: Mật khẩu cũ đang trống")]
    [InlineData("oldpassword123", "", "NewPass: Mật khẩu mới đang trống")]
    [InlineData("oldpassword123", "123", "NewPass: Mật khẩu dài từ 6-255 ký tự")]
    public async Task ChangePassword_InvalidInput_ReturnsBadRequest(string oldPass, string newPass,
        string expectedError)
    {
        await AuthenticateAsync();
        var command = new ChangePasswordCommand { OldPass = oldPass, NewPass = newPass };

        var response = await Client.PatchAsJsonAsync("api/account/password", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Contain(expectedError);
    }

    [Fact]
    public async Task ChangePassword_NoToken_ReturnsUnauthorized()
    {
        var command = new ChangePasswordCommand { OldPass = "oldpass123", NewPass = "newpass123" };
        var response = await Client.PatchAsJsonAsync("api/account/password", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_UserNotFound_ReturnsNotFound()
    {
        await AuthenticateAsync("nonexistentuser");
        var command = new ChangePasswordCommand { OldPass = "oldpass123", NewPass = "newpass123" };

        var response = await Client.PatchAsJsonAsync("api/account/password", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("same_pass", "password123", "password123", "Mật khẩu mới không được giống mật khẩu cũ")]
    [InlineData("third_party", "any_old", "newpassword123",
        "Tài khoản chưa thiết lập mật khẩu, không thể đổi mật khẩu")]
    [InlineData("wrong_old", "wrongpassword", "newpassword123", "Mật khẩu cũ không đúng")]
    public async Task ChangePassword_LogicError_ReturnsUnprocessableEntity(string scenario, string oldPass,
        string newPass, string expectedError)
    {
        var id = Guid.NewGuid().ToString("N").Substring(0, 12);
        var isThirdParty = scenario == "third_party";
        var dbPass = scenario == "same_pass" ? "password123" : "correctpassword123";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Accounts.Add(new Domain.Entities.Account
            {
                Mail = $"{id}@test.com",
                Pass = isThirdParty ? null : BC.HashPassword(dbPass),
                IsThirdParty = isThirdParty,
                Role = RoleEnum.user,
                Status = StatusAccountEnum.active,
                UserNavi = new User { UserId = id, Name = "Logic Test User" }
            });
            await db.SaveChangesAsync();
        }

        await AuthenticateAsync(id);
        var command = new ChangePasswordCommand { OldPass = oldPass, NewPass = newPass };

        var response = await Client.PatchAsJsonAsync("api/account/password", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task ChangePassword_ValidCommand_ReturnsOk()
    {
        var id = Guid.NewGuid().ToString("N").Substring(0, 12);
        var oldPass = "oldpassword123";
        var newPass = "111111";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Accounts.Add(new Domain.Entities.Account
            {
                Mail = $"{id}@success.com",
                Pass = BC.HashPassword(oldPass),
                IsThirdParty = false,
                Role = RoleEnum.user,
                Status = StatusAccountEnum.active,
                UserNavi = new User { UserId = id, Name = "Success Test User" }
            });
            await db.SaveChangesAsync();
        }

        await AuthenticateAsync(id);
        var command = new ChangePasswordCommand { OldPass = oldPass, NewPass = newPass };

        var response = await Client.PatchAsJsonAsync("api/account/password", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Đổi mật khẩu thành công");

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            var account = await db.Accounts.Include(a => a.UserNavi).FirstOrDefaultAsync(a => a.UserNavi.UserId == id);
            BC.Verify(newPass, account!.Pass).Should().BeTrue();
        }
    }
}