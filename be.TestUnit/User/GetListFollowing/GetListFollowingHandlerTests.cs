using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Follow;
using be.Application.Features.User.GetListFollowing;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.GetListFollowing;

public class GetListFollowingHandlerTests
{
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly GetListFollowingHandler _handler;
    private readonly IUserRepository _userRepository;

    public GetListFollowingHandlerTests()
    {
        _format = Substitute.For<IFormat>();
        _userRepository = Substitute.For<IUserRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _encryption = Substitute.For<IEncryption>();

        _handler = new GetListFollowingHandler(_format, _userRepository, _followReadRepository, _encryption);
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowNotFoundException()
    {
        _userRepository.GetPrivateIdByPublicIdAsync("target-123", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        var query = new GetListFollowingQuery("target-123", null, 10);

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại tài khoản 'target-123'");
    }

    [Fact]
    public async Task Handle_WhenValidRequestWithoutCursor_ShouldReturnData()
    {
        _userRepository.GetPrivateIdByPublicIdAsync("target-123", Arg.Any<CancellationToken>())
            .Returns((short)10);

        var avatarGuid = Guid.NewGuid();
        var docs = new List<Follow2Dto>
        {
            new()
            {
                UserId = "u1", UserName = "User1", Avatar = avatarGuid, CreatedAt = DateTimeOffset.UtcNow, Sequence = 1
            }
        };

        var nextCursorPayload = new CursorPayload<DateTimeOffset>(DateTimeOffset.UtcNow, 1);
        var queryResult = new CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>(docs, true, nextCursorPayload);

        _followReadRepository.GetListFolloweeAsync(10, 10, null, Arg.Any<CancellationToken>())
            .Returns(queryResult);

        _format.FormatImageUrl(avatarGuid, "User1").Returns("http://img/avatar1.png");
        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted-cursor");

        var query = new GetListFollowingQuery("target-123", null, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Users.Should().HaveCount(1);
        result.Data.Users[0].UserId.Should().Be("u1");
        result.Data.Users[0].Avatar.Should().Be("http://img/avatar1.png");
        result.Data.PageProfile.HasNextPage.Should().BeTrue();
        result.Data.PageProfile.NextCursor.Should().Be("encrypted-cursor");
    }

    [Fact]
    public async Task Handle_WhenValidRequestWithCursor_ShouldDecryptAndReturnData()
    {
        _userRepository.GetPrivateIdByPublicIdAsync("target-123", Arg.Any<CancellationToken>())
            .Returns((short)10);

        var cursorPayload = new CursorPayload<DateTimeOffset>(DateTimeOffset.UtcNow, 1);
        var cursorJson = JsonSerializer.Serialize(cursorPayload);
        _encryption.Decrypt("encrypted-cursor-input").Returns(cursorJson);

        var docs = new List<Follow2Dto>();
        var queryResult = new CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>(docs, false, null);

        _followReadRepository
            .GetListFolloweeAsync(10, 10, Arg.Any<CursorPayload<DateTimeOffset>>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        var query = new GetListFollowingQuery("target-123", "encrypted-cursor-input", 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Users.Should().BeEmpty();
        result.Data.PageProfile.HasNextPage.Should().BeFalse();
        result.Data.PageProfile.NextCursor.Should().BeNull();
    }
}