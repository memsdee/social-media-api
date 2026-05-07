using be.Application.Features.Notification.GetUnreadCount;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.Notification.GetUnreadCount;

public class GetUnreadCountHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GetUnreadCountHandler _handler;
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly IUserRepository _userRepository;

    public GetUnreadCountHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _notificationReadRepository = Substitute.For<INotificationReadRepository>();

        _handler = new GetUnreadCountHandler(
            _currentUserContext,
            _userRepository,
            _notificationReadRepository
        );
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenUnreadCountIsBelowCap_ShouldReturnActualCount()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)12);
        _notificationReadRepository.GetUnreadCountAsync(12, Arg.Any<CancellationToken>())
            .Returns(7);

        var result = await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WhenUnreadCountExceedsCap_ShouldReturnNinetyNine()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)12);
        _notificationReadRepository.GetUnreadCountAsync(12, Arg.Any<CancellationToken>())
            .Returns(100);

        var result = await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(99);
    }
}