using be.Application.Dtos.EventBus;
using be.Application.Features.Post.Post.DeletePost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Post.DeletePost;

public class DeletePostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly DeletePostHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public DeletePostHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new DeletePostHandler(
            _userRepository,
            _postRepository,
            _unitOfWork,
            _outboxRepository,
            _currentUserContext);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DeletePostCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DeletePostCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUpdateStatusIsZero_ShouldThrowNotFoundAndRollback()
    {
        var postId = Guid.NewGuid();
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)7);
        _postRepository.UpdateStatusAsync(postId, 7, StatusPostEnum.deleted, Arg.Any<CancellationToken>())
            .Returns(0);

        Func<Task> act = async () => await _handler.Handle(new DeletePostCommand(postId), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Bài viết không tồn tại hoặc bạn không có quyền xóa bài viết này");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldAddOutboxSaveAndCommit()
    {
        var postId = Guid.NewGuid();
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)7);
        _postRepository.UpdateStatusAsync(postId, 7, StatusPostEnum.deleted, Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(new DeletePostCommand(postId), CancellationToken.None);

        result.Message.Should().Be("Xóa bài viết thành công");
        await _outboxRepository.Received(1).AddAsync(
            OutboxTopicEnum.postDelByScore,
            Arg.Is<PostDeletedByScoreEvent>(e =>
                e.Status == StatusPostEnum.deleted &&
                e.ListIdPublicPost.Count == 1 &&
                e.ListIdPublicPost[0] == postId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        var postId = Guid.NewGuid();
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)7);
        _postRepository.UpdateStatusAsync(postId, 7, StatusPostEnum.deleted, Arg.Any<CancellationToken>())
            .Returns(1);
        _outboxRepository.AddAsync(OutboxTopicEnum.postDelByScore, Arg.Any<PostDeletedByScoreEvent>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () => await _handler.Handle(new DeletePostCommand(postId), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}