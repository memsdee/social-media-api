using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.User.Follow;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace be.Tests.Unit.Features.User;

public class FollowHandlerTests
{
    private static readonly string FollowerUserId = "user-follower-001";
    private static readonly string FolloweeUserId = "user-followee-002";

    private static readonly User2Dto FakeFollower = new()
    {
        SequenceId = 1,
        PublicUserId = FollowerUserId,
        Name = "Follower User",
        Avatar = Guid.NewGuid(),
        IsDeleteAccount = false
    };

    private static readonly User2Dto FakeFollowee = new()
    {
        SequenceId = 2,
        PublicUserId = FolloweeUserId,
        Name = "Followee User",
        Avatar = Guid.NewGuid(),
        IsDeleteAccount = false
    };

    private readonly ICurrentUserContext _currentUserContext = Substitute.For<ICurrentUserContext>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();

    private readonly FollowHandler _handler;
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IOutboxRepository _outboxRepository = Substitute.For<IOutboxRepository>();
    private readonly IRealtimeNotifier _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
    private readonly ITransaction _transaction = Substitute.For<ITransaction>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    public FollowHandlerTests()
    {
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(_transaction);

        _handler = new FollowHandler(
            _realtimeNotifier,
            _currentUserContext,
            _unitOfWork,
            _userRepository,
            _followRepository,
            _notificationRepository,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_ValidFollow_ReturnsSuccessResponse()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>()).Returns(FakeFollowee);

        var command = new FollowCommand(FolloweeUserId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Theo dõi người dùng thành công", result.Message);
    }

    [Fact]
    public async Task Handle_ValidFollow_InsertsFollowAndNotificationToOutbox()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>()).Returns(FakeFollowee);

        var command = new FollowCommand(FolloweeUserId);

        await _handler.Handle(command, CancellationToken.None);

        await _outboxRepository.Received(1)
            .AddAsync(OutboxTopicEnum.follow, Arg.Any<FollowEvent>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1)
            .AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiFollowEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidFollow_SavesChangesAndCommitsTransaction()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>()).Returns(FakeFollowee);

        var command = new FollowCommand(FolloweeUserId);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidFollow_SendsRealtimeNotificationToFollowee()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>()).Returns(FakeFollowee);

        var command = new FollowCommand(FolloweeUserId);

        await _handler.Handle(command, CancellationToken.None);

        await _realtimeNotifier.Received(1)
            .SendToUserAsync(FolloweeUserId, Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FollowSelf_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);

        var command = new FollowCommand(FollowerUserId);

        await Assert.ThrowsAsync<CustomException.BusinessValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserIdNull_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        var command = new FollowCommand(FolloweeUserId);

        await Assert.ThrowsAsync<CustomException.UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_FollowerNotFound_ThrowsNotFoundException()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>())
            .Returns((User2Dto?)null);

        var command = new FollowCommand(FolloweeUserId);

        await Assert.ThrowsAsync<CustomException.NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_FolloweeNotFound_ThrowsNotFoundException()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>())
            .Returns((User2Dto?)null);

        var command = new FollowCommand(FolloweeUserId);

        await Assert.ThrowsAsync<CustomException.NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SaveChangesFails_RollsBackTransaction()
    {
        _currentUserContext.UserId.Returns(FollowerUserId);
        _userRepository.GetUser2Async(FollowerUserId, Arg.Any<CancellationToken>()).Returns(FakeFollower);
        _userRepository.GetUser2Async(FolloweeUserId, Arg.Any<CancellationToken>()).Returns(FakeFollowee);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("DB error"));

        var command = new FollowCommand(FolloweeUserId);

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}