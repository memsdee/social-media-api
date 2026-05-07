using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Follow;
using be.Application.Features.User.GetListFollowers;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.GetListFollowers;

public class GetListFollowersHandlerTests
{
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly GetListFollowersHandler _handler;
    private readonly IUserRepository _userRepository;

    public GetListFollowersHandlerTests()
    {
        _format = Substitute.For<IFormat>();
        _userRepository = Substitute.For<IUserRepository>();
        _encryption = Substitute.For<IEncryption>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();

        _handler = new GetListFollowersHandler(
            _format,
            _userRepository,
            _encryption,
            _followReadRepository);
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowNotFoundException()
    {
        var targetId = "non-existent";
        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new GetListFollowersQuery(targetId, null, 10), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage($"Không tồn tại tài khoản '{targetId}'");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnFollowers()
    {
        var targetId = "target-public";
        var targetPrivateId = (short)10;
        var followers = new List<Follow2Dto>
        {
            new()
            {
                UserId = "follower-1", UserName = "Alice", Avatar = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow
            }
        };
        var cursorResult = new CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>(followers, false, null);

        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(targetPrivateId);
        _followReadRepository.GetListFollowerAsync(targetPrivateId, 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("formatted-avatar");

        var result = await _handler.Handle(new GetListFollowersQuery(targetId, null, 10), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Users.Should().HaveCount(1);
        result.Data.Users[0].UserId.Should().Be("follower-1");
        result.Data.Users[0].Avatar.Should().Be("formatted-avatar");
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldDecryptAndUseCursor()
    {
        var targetId = "target-public";
        var targetPrivateId = (short)10;
        var encryptedCursor = "encrypted-cursor";
        var cursor = new CursorPayload<DateTimeOffset>(DateTimeOffset.UtcNow, 5);

        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(targetPrivateId);
        _encryption.Decrypt(encryptedCursor).Returns(JsonSerializer.Serialize(cursor));
        _followReadRepository.GetListFollowerAsync(targetPrivateId, 10,
                Arg.Is<CursorPayload<DateTimeOffset>?>(c => c != null && c.Id == 5), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>(new List<Follow2Dto>(), false, null));

        await _handler.Handle(new GetListFollowersQuery(targetId, encryptedCursor, 10), CancellationToken.None);

        await _followReadRepository.Received(1).GetListFollowerAsync(targetPrivateId, 10,
            Arg.Any<CursorPayload<DateTimeOffset>?>(), Arg.Any<CancellationToken>());
    }
}