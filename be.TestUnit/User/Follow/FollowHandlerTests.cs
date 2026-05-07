using be.Application.Common.Constants;
using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.User.Follow;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.Follow;

public class FollowHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFollowRepository _followRepository;
    private readonly FollowHandler _handler;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public FollowHandlerTests()
    {
        _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _followRepository = Substitute.For<IFollowRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _followRepository.AddAsync(Arg.Any<be.Domain.Entities.Follow>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notificationRepository.AddAsync(Arg.Any<Notifications>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository.AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<FollowEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository.AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<NotiFollowEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _realtimeNotifier.SendToUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

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
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new FollowCommand("target"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenSelfFollow_ShouldThrowBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-a");

        Func<Task> act = async () => await _handler.Handle(new FollowCommand("user-a"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>();
    }

    [Fact]
    public async Task Handle_WhenFollowerNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-a");
        _userRepository.GetUser2Async("user-a", Arg.Any<CancellationToken>()).Returns((User2Dto?)null);

        Func<Task> act = async () => await _handler.Handle(new FollowCommand("user-b"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFolloweeNotFound_ShouldThrowNotFoundException()
    {
        var follower = new User2Dto
        {
            PublicUserId = "user-a",
            Name = "User A",
            SequenceId = 1,
            Avatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        _currentUserContext.UserId.Returns("user-a");
        _userRepository.GetUser2Async("user-a", Arg.Any<CancellationToken>()).Returns(follower);
        _userRepository.GetUser2Async("user-b", Arg.Any<CancellationToken>()).Returns((User2Dto?)null);

        Func<Task> act = async () => await _handler.Handle(new FollowCommand("user-b"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateFollowAndNotify()
    {
        var follower = new User2Dto
        {
            PublicUserId = "user-a",
            Name = "User A",
            SequenceId = 1,
            Avatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        var followee = new User2Dto
        {
            PublicUserId = "user-b",
            Name = "User B",
            SequenceId = 2,
            Avatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        _currentUserContext.UserId.Returns("user-a");
        _userRepository.GetUser2Async("user-a", Arg.Any<CancellationToken>()).Returns(follower);
        _userRepository.GetUser2Async("user-b", Arg.Any<CancellationToken>()).Returns(followee);

        var result = await _handler.Handle(new FollowCommand("user-b"), CancellationToken.None);

        result.Should().NotBeNull();
        await _followRepository.Received(1).AddAsync(
            Arg.Is<be.Domain.Entities.Follow>(model => model.FollowerId == 1 && model.FolloweeId == 2),
            Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notifications>(model =>
                model.SenderId == 1 &&
                model.ReciverId == 2 &&
                model.Target == NotiTargetEnum.user &&
                model.Action == NotiActionEnum.follow),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(
            OutboxTopicEnum.follow,
            Arg.Any<FollowEvent>(),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(
            OutboxTopicEnum.notification,
            Arg.Any<NotiFollowEvent>(),
            Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _realtimeNotifier.Received(1).SendToUserAsync(
            "user-b",
            SignalRMethods.Notification.New,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldRollback()
    {
        var follower = new User2Dto
        {
            PublicUserId = "user-a",
            Name = "User A",
            SequenceId = 1,
            Avatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        var followee = new User2Dto
        {
            PublicUserId = "user-b",
            Name = "User B",
            SequenceId = 2,
            Avatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        _currentUserContext.UserId.Returns("user-a");
        _userRepository.GetUser2Async("user-a", Arg.Any<CancellationToken>()).Returns(follower);
        _userRepository.GetUser2Async("user-b", Arg.Any<CancellationToken>()).Returns(followee);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () => await _handler.Handle(new FollowCommand("user-b"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _realtimeNotifier.DidNotReceive().SendToUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }
}