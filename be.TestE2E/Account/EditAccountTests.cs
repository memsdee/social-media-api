using System.Net;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.Change.EditAccount;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using FluentAssertions;

namespace be.TestE2E.Account;

public class EditAccountTests : BaseE2ETest
{
    public EditAccountTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<string> CreateDummyUserAsync(string userId, string name)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WriteContext>();

        if (!db.Users.Any(u => u.UserId == userId))
        {
            var account = new Domain.Entities.Account
            {
                Mail = $"{userId}@test.com",
                Pass = "password123",
                Role = RoleEnum.user,
                Status = StatusAccountEnum.active,
                UserNavi = new User
                {
                    UserId = userId,
                    Name = name,
                    Bio = "SameBio"
                }
            };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
        }

        return userId;
    }

    [Fact]
    public async Task EditAccount_WhenAllFieldsNull_Returns400()
    {
        await AuthenticateAsync();
        var command = new EditAccountCommand();
        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Contain("Vui lòng cung cấp ít nhất một trường để cập nhật");
    }

    [Theory]
    [InlineData("ab", null, null, "UserName: Độ dài UserName từ 3-50 ký tự")]
    [InlineData(null, "invalid id @", null, "UserId: Userid chỉ được chứa chữ cái, số và dấu gạch dưới")]
    [InlineData(null, null,
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        "Bio: Độ dài Bio tối đa 160 ký tự")]
    public async Task EditAccount_WhenValidationFails_Returns400(string? userName, string? userId, string? bio,
        string expectedMessage)
    {
        await AuthenticateAsync();
        var command = new EditAccountCommand { UserName = userName, UserId = userId, Bio = bio };
        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Contain(expectedMessage);
    }

    [Fact]
    public async Task EditAccount_WhenNotAuthenticated_Returns401()
    {
        var command = new EditAccountCommand { UserName = "New Name" };
        var response = await Client.PatchAsJsonAsync("api/account", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditAccount_WhenUserNotInDb_ReturnsNotFound()
    {
        await AuthenticateAsync("non-existent-user");
        var command = new EditAccountCommand { UserName = "New Name" };
        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Người dùng không tồn tại");
    }

    [Theory]
    [InlineData("Dummy Name", null, null, "Tên mới không được giống tên cũ")]
    [InlineData(null, "dummy_id_same", null, "Id mới không được giống Id cũ")]
    public async Task EditAccount_WhenFieldSameAsOld_Returns422(string? userName, string? userId, string? bio,
        string expectedMessage)
    {
        var dummyId = userId ?? Guid.NewGuid().ToString("N")[..10];
        await CreateDummyUserAsync(dummyId, "Dummy Name");
        await AuthenticateAsync(dummyId);

        var command = new EditAccountCommand { UserName = userName, UserId = userId, Bio = bio };
        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task EditAccount_WhenUserIdAlreadyExists_Returns422()
    {
        await CreateDummyUserAsync("user_taken", "Taken User");
        var dummyId = Guid.NewGuid().ToString("N")[..10];
        await CreateDummyUserAsync(dummyId, "Trying User");

        await AuthenticateAsync(dummyId);
        var command = new EditAccountCommand { UserId = "user_taken" };
        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Id này đã tồn tại, vui lòng chọn Id khác");
    }

    [Fact]
    public async Task EditAccount_WhenUserNameChangedWithin7Days_Returns422()
    {
        var dummyId = Guid.NewGuid().ToString("N")[..10];
        await CreateDummyUserAsync(dummyId, "Name Limit");
        await AuthenticateAsync(dummyId);

        var command1 = new EditAccountCommand { UserName = "Name Limit Temp" };
        var response1 = await Client.PatchAsJsonAsync("api/account", command1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var command2 = new EditAccountCommand { UserName = "Another Name" };
        var response2 = await Client.PatchAsJsonAsync("api/account", command2);

        response2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response2.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("1 tuần chỉ đổi tên được 1 lần");
    }

    [Fact]
    public async Task EditAccount_WhenUserIdChangedWithin30Days_Returns422()
    {
        var dummyId = Guid.NewGuid().ToString("N")[..10];
        await CreateDummyUserAsync(dummyId, "ID Limit");
        await AuthenticateAsync(dummyId);

        var newId = Guid.NewGuid().ToString("N")[..10];
        var command1 = new EditAccountCommand { UserId = newId };
        var response1 = await Client.PatchAsJsonAsync("api/account", command1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        await AuthenticateAsync(newId);

        var command2 = new EditAccountCommand { UserId = Guid.NewGuid().ToString("N")[..10] };
        var response2 = await Client.PatchAsJsonAsync("api/account", command2);

        response2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response2.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("1 tháng chỉ đổi Id được 1 lần");
    }

    [Fact]
    public async Task EditAccount_WhenValid_Returns200()
    {
        var dummyId = Guid.NewGuid().ToString("N")[..10];
        await CreateDummyUserAsync(dummyId, "Dummy Happy");
        await AuthenticateAsync(dummyId);

        var command = new EditAccountCommand
        {
            UserName = "New Happy Name",
            Bio = "Updated happy bio"
        };

        var response = await Client.PatchAsJsonAsync("api/account", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BaseResponse>();
        result!.Message.Should().Be("Cập nhật thông tin tài khoản thành công");
    }
}