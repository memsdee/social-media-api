using be.Application.Dtos.Queries.User;
using be.Application.Features.User.GetMe;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.GetMe;

public class GetMeHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFormat _format;
    private readonly GetMeHandler _handler;
    private readonly IUserReadRepository _userReadRepository;

    public GetMeHandlerTests()
    {
        _userReadRepository = Substitute.For<IUserReadRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _format = Substitute.For<IFormat>();

        _handler = new GetMeHandler(
            _userReadRepository,
            _currentUserContext,
            _format);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
        await _userReadRepository.DidNotReceive()
            .GetUser3Async(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("me");
        _userReadRepository.GetUser3Async("me", Arg.Any<CancellationToken>())
            .Returns((User3Dto?)null);

        Func<Task> act = async () => await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnAvatar()
    {
        _currentUserContext.UserId.Returns("me");

        var user = new User3Dto
        {
            PublicUserId = "me",
            Name = "Me",
            Avatar = Guid.NewGuid()
        };

        _userReadRepository.GetUser3Async("me", Arg.Any<CancellationToken>())
            .Returns(user);
        _format.FormatImageUrl(user.Avatar, user.PublicUserId).Returns("avatar-url");

        var result = await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Avatar.Should().Be("avatar-url");
    }
}