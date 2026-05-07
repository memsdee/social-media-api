using be.Application.Dtos.Queries.User;
using be.Application.Features.User.GetProfile;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.GetProfile;

public class GetProfileHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly GetProfileHandler _handler;
    private readonly IUserReadRepository _userReadRepository;

    public GetProfileHandlerTests()
    {
        _userReadRepository = Substitute.For<IUserReadRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _format = Substitute.For<IFormat>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();

        _handler = new GetProfileHandler(
            _userReadRepository,
            _currentUserContext,
            _format,
            _followReadRepository);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new GetProfileQuery { UserId = "user-1" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
        await _userReadRepository.DidNotReceive().GetUser4Async(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("me");
        _userReadRepository.GetUser4Async("target", "me", false, Arg.Any<CancellationToken>())
            .Returns((User4Dto?)null);
        _followReadRepository.IsFollowingAsync("me", "target", Arg.Any<CancellationToken>())
            .Returns(false);

        Func<Task> act = async () =>
            await _handler.Handle(new GetProfileQuery { UserId = "target" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại tài khoản 'target' ");
    }

    [Fact]
    public async Task Handle_WhenMyProfile_ShouldReturnProfileWithoutFollowingCheck()
    {
        _currentUserContext.UserId.Returns("me");

        var user = new User4Dto
        {
            PublicUserId = "me",
            Name = "My Name",
            Avatar = Guid.NewGuid(),
            TotalPost = 5,
            TotalFollower = 9,
            TotalFollowing = 3
        };

        _userReadRepository.GetUser4Async("me", "me", true, Arg.Any<CancellationToken>())
            .Returns(user);
        _format.FormatImageUrl(user.Avatar, user.PublicUserId).Returns("avatar-url");

        var result = await _handler.Handle(new GetProfileQuery { UserId = "me" }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be("me");
        result.Data.UserName.Should().Be("My Name");
        result.Data.Avatar.Should().Be("avatar-url");
        result.Data.TotalPost.Should().Be(5);
        result.Data.TotalFollower.Should().Be(9);
        result.Data.TotalFollowing.Should().Be(3);
        result.Data.Metadata.IsMyProfile.Should().BeTrue();
        result.Data.Metadata.IsFollowing.Should().BeFalse();

        await _followReadRepository.DidNotReceive().IsFollowingAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOtherProfile_ShouldReturnProfileWithFollowingStatus()
    {
        _currentUserContext.UserId.Returns("me");

        var user = new User4Dto
        {
            PublicUserId = "target",
            Name = "Target",
            Avatar = Guid.NewGuid(),
            TotalPost = 2,
            TotalFollower = 4,
            TotalFollowing = 1
        };

        _userReadRepository.GetUser4Async("target", "me", false, Arg.Any<CancellationToken>())
            .Returns(user);
        _followReadRepository.IsFollowingAsync("me", "target", Arg.Any<CancellationToken>())
            .Returns(true);
        _format.FormatImageUrl(user.Avatar, user.PublicUserId).Returns("target-avatar-url");

        var result = await _handler.Handle(new GetProfileQuery { UserId = "target" }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be("target");
        result.Data.UserName.Should().Be("Target");
        result.Data.Avatar.Should().Be("target-avatar-url");
        result.Data.Metadata.IsMyProfile.Should().BeFalse();
        result.Data.Metadata.IsFollowing.Should().BeTrue();

        await _followReadRepository.Received(1).IsFollowingAsync("me", "target", Arg.Any<CancellationToken>());
    }
}