using be.Application.Dtos.EventBus;
using be.Application.Features.User.Unfollow;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.Unfollow;

public class UnfollowHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFollowRepository _followRepository;
    private readonly UnfollowHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UnfollowHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _followRepository = Substitute.For<IFollowRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _outboxRepository.AddAsync(OutboxTopicEnum.unFollow, Arg.Any<UnfollowEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _transaction.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _transaction.RollbackAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        _handler = new UnfollowHandler(
            _currentUserContext,
            _userRepository,
            _followRepository,
            _unitOfWork,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenSelfUnfollow_ShouldThrowBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("me");

        Func<Task> act = async () => await _handler.Handle(new UnfollowCommand("me"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Bạn không thể hủy theo dõi chính mình!");
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetPrivateIdByPublicIdAsync("target", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Không tồn tại người dùng");
    }

    [Fact]
    public async Task Handle_WhenCurrentUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetPrivateIdByPublicIdAsync("target", Arg.Any<CancellationToken>())
            .Returns((short)20);
        _userRepository.GetPrivateIdByPublicIdAsync("me", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenNotFollowing_ShouldReturnNotFollowingMessage()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetPrivateIdByPublicIdAsync("target", Arg.Any<CancellationToken>())
            .Returns((short)20);
        _userRepository.GetPrivateIdByPublicIdAsync("me", Arg.Any<CancellationToken>())
            .Returns((short)10);
        _followRepository.UnFollowAsync(10, 20, Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        result.Message.Should().Be("Bạn chưa theo dõi người dùng này!");
        await _followRepository.Received(1).UnFollowAsync(10, 20, Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().AddAsync(OutboxTopicEnum.unFollow, Arg.Any<UnfollowEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnfollowed_ShouldAddOutboxSaveAndReturnSuccess()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetPrivateIdByPublicIdAsync("target", Arg.Any<CancellationToken>())
            .Returns((short)20);
        _userRepository.GetPrivateIdByPublicIdAsync("me", Arg.Any<CancellationToken>())
            .Returns((short)10);
        _followRepository.UnFollowAsync(10, 20, Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        result.Message.Should().Be("Hủy theo dõi thành công!");
        await _followRepository.Received(1).UnFollowAsync(10, 20, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(
            OutboxTopicEnum.unFollow,
            Arg.Is<UnfollowEvent>(e => e.FollowerSequenceId == 10 && e.FolloweeSequenceId == 20),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetPrivateIdByPublicIdAsync("target", Arg.Any<CancellationToken>())
            .Returns((short)20);
        _userRepository.GetPrivateIdByPublicIdAsync("me", Arg.Any<CancellationToken>())
            .Returns((short)10);
        _followRepository.UnFollowAsync(10, 20, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("db error")));

        Func<Task> act = async () => await _handler.Handle(new UnfollowCommand("target"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}