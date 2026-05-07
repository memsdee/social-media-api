using System.Net;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.Change.ChangeEmail;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using BC = BCrypt.Net.BCrypt;

namespace be.TestE2E.Account;

public class ChangeEmailTests : BaseE2ETest
{
    public ChangeEmailTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("", "123456", "password123", "NewEmail: Mail không để trống")]
    [InlineData("long_email", "123456", "password123", "NewEmail: Độ dài email tối đa 255 ký tự")]
    [InlineData("invalid-email", "123456", "password123", "NewEmail: Định dạng mail không hợp lệ")]
    [InlineData("valid@mail.com", "", "password123", "Otp: OTP đang trống")]
    [InlineData("valid@mail.com", "123456", "", "Pass: Mật khẩu đang trống")]
    [InlineData("valid@mail.com", "123456", "123", "Pass: Mật khẩu dài từ 6-255 ký tự")]
    public async Task ChangeEmail_InvalidInput_ReturnsBadRequest(string email, string otp, string pass,
        string expectedError)
    {
        await AuthenticateAsync();
        if (email == "long_email") email = new string('a', 250) + "@mail.com";

        var command = new ChangeEmailCommand { NewEmail = email, Otp = otp, Pass = pass };
        var response = await Client.PatchAsJsonAsync("api/account/mail", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Contain(expectedError);
    }

    [Fact]
    public async Task ChangeEmail_NoToken_Returns401()
    {
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456", Pass = "password123" };
        var response = await Client.PatchAsJsonAsync("api/account/mail", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeEmail_UserNotFound_Returns401()
    {
        await AuthenticateAsync("nonexistentuser");
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456", Pass = "password123" };
        var response = await Client.PatchAsJsonAsync("api/account/mail", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Vui lòng đăng nhập lại");
    }

    [Theory]
    [InlineData("none", "123456", "password123", "OTP không đúng hoặc đã hết hạn", false, false)]
    [InlineData("wrong", "000000", "password123", "OTP không đúng hoặc đã hết hạn", true, false)]
    [InlineData("tp", "123456", "password123", "Tài khoản chỉ đăng nhập bằng bên thứ 3, không thể đổi email", true,
        true)]
    [InlineData("same", "123456", "password123", "Email mới không được trùng với email hiện tại", true, false)]
    [InlineData("valid", "123456", "wrongpass", "Mật khẩu không đúng", true, false)]
    public async Task ChangeEmail_BusinessLogic_Returns422(string scenario, string otp, string pass,
        string expectedError, bool hasOtp, bool isThirdParty)
    {
        var id = Guid.NewGuid().ToString("N").Substring(0, 10);
        var mail = $"{id}@mini4rum.com";
        var newMail = scenario == "same" ? mail : $"new_{id}@mini4rum.com";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            var account = new Domain.Entities.Account
            {
                Mail = mail,
                Pass = isThirdParty ? null : BC.HashPassword("password123"),
                IsThirdParty = isThirdParty,
                Role = RoleEnum.user,
                Status = StatusAccountEnum.active,
                UserNavi = new User { UserId = $"u_{id}", Name = "Valid User Name" }
            };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
        }

        await AuthenticateAsync($"u_{id}");
        if (hasOtp) await SetOtpInRedis(newMail, otp == "000000" ? "111222" : otp);

        var command = new ChangeEmailCommand { NewEmail = newMail, Otp = otp, Pass = pass };
        var response = await Client.PatchAsJsonAsync("api/account/mail", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be(expectedError);
    }

    [Fact]
    public async Task ChangeEmail_Success_Returns200()
    {
        var id = Guid.NewGuid().ToString("N").Substring(0, 10);
        var oldMail = $"{id}@old.com";
        var newMail = $"{id}@aaaaa.com";
        var pass = "password123";
        var otp = "999000";

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            db.Accounts.Add(new Domain.Entities.Account
            {
                Mail = oldMail,
                Pass = BC.HashPassword(pass),
                IsThirdParty = false,
                Role = RoleEnum.user,
                Status = StatusAccountEnum.active,
                UserNavi = new User { UserId = $"u_{id}", Name = "Success User Name" }
            });
            await db.SaveChangesAsync();
        }

        await AuthenticateAsync($"u_{id}");
        await SetOtpInRedis(newMail, otp);

        var command = new ChangeEmailCommand { NewEmail = newMail, Otp = otp, Pass = pass };
        var response = await Client.PatchAsJsonAsync("api/account/mail", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Đổi email thành công");

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteContext>();
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.UserNavi.UserId == $"u_{id}");
            account!.Mail.Should().Be(newMail);
        }
    }

    private async Task SetOtpInRedis(string key, string otp)
    {
        using var scope = Factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.SetStringAsync(key, otp);
    }
}