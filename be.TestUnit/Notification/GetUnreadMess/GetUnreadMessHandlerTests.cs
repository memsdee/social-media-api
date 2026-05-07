using be.Application.Features.Notification.GetUnreadMess;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;

namespace Tests.Notification.GetUnreadMess;

public class GetUnreadMessHandlerTests
{
    private readonly IConversationReadRepository _conversationReadRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GetUnreadMessHandler _handler;
    private readonly IUserRepository _userRepository;

    public GetUnreadMessHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _conversationReadRepository = Substitute.For<IConversationReadRepository>();

        _handler = new GetUnreadMessHandler(
            _currentUserContext,
            _userRepository,
            _conversationReadRepository);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new GetUnreadMessQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new GetUnreadMessQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnTotalUnreadCount()
    {
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _conversationReadRepository.GetTotalUnreadCountAsync(1, Arg.Any<CancellationToken>()).Returns(5);

        var result = await _handler.Handle(new GetUnreadMessQuery(), CancellationToken.None);

        result.Data.Should().Be(5);
        await _conversationReadRepository.Received(1).GetTotalUnreadCountAsync(1, Arg.Any<CancellationToken>());
    }
}