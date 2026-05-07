using be.Application.Dtos.EventBus;
using be.Application.Features.Post.Comment.DeleteComment;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Comment.DeleteComment;

public class DeleteCommentHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly DeleteCommentHandler _handler;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public DeleteCommentHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _commentRepository = Substitute.For<ICommentRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new DeleteCommentHandler(
            _currentUserContext,
            _unitOfWork,
            _userRepository,
            _commentRepository,
            _notificationRepository,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DeleteCommentCommand(Guid.NewGuid()), CancellationToken.None);

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
            await _handler.Handle(new DeleteCommentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _commentRepository.GetByIdPublicAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((be.Domain.Entities.Comment?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DeleteCommentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Bình luận không tồn tại");
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ShouldThrowForbiddenException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>()).Returns((short)1);

        var comment = new be.Domain.Entities.Comment { UserId = 2, IdPublic = Guid.NewGuid() };
        _commentRepository.GetByIdPublicAsync(comment.IdPublic, Arg.Any<CancellationToken>()).Returns(comment);

        Func<Task> act = async () =>
            await _handler.Handle(new DeleteCommentCommand(comment.IdPublic), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.ForbiddenException>()
            .WithMessage("Bạn không có quyền xóa bình luận này");
    }

    [Fact]
    public async Task Handle_WhenSuccessWithNotifications_ShouldDeleteAndCommit()
    {
        var publicUserId = "user-public-id";
        var privateUserId = (short)1;
        var commentIdPublic = Guid.NewGuid();
        var commentIdPrivate = (short)10;

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetPrivateIdByPublicIdAsync(publicUserId, Arg.Any<CancellationToken>()).Returns(privateUserId);

        var comment = new be.Domain.Entities.Comment
            { Id = commentIdPrivate, UserId = privateUserId, IdPublic = commentIdPublic };
        _commentRepository.GetByIdPublicAsync(commentIdPublic, Arg.Any<CancellationToken>()).Returns(comment);

        var notiIds = new List<short> { 100, 101 };
        _commentRepository.GetNotiIdsByCommentIdAsync(commentIdPrivate, Arg.Any<CancellationToken>()).Returns(notiIds);

        var result = await _handler.Handle(new DeleteCommentCommand(commentIdPublic), CancellationToken.None);

        result.Message.Should().Be("Xóa bình luận thành công");
        await _notificationRepository.Received(1).DeleteByIdsAsync(notiIds, Arg.Any<CancellationToken>());
        _commentRepository.Received(1).Delete(comment);
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.comment, Arg.Any<CommentDeletedEvent>(),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(2).AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiDeletedEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoNotificationIds_ShouldNotDeleteNotifications()
    {
        var publicUserId = "user-public-id";
        var privateUserId = (short)1;
        var commentIdPublic = Guid.NewGuid();
        var commentIdPrivate = (short)10;

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetPrivateIdByPublicIdAsync(publicUserId, Arg.Any<CancellationToken>()).Returns(privateUserId);

        var comment = new be.Domain.Entities.Comment
            { Id = commentIdPrivate, UserId = privateUserId, IdPublic = commentIdPublic };
        _commentRepository.GetByIdPublicAsync(commentIdPublic, Arg.Any<CancellationToken>()).Returns(comment);
        _commentRepository.GetNotiIdsByCommentIdAsync(commentIdPrivate, Arg.Any<CancellationToken>())
            .Returns(new List<short>());

        await _handler.Handle(new DeleteCommentCommand(commentIdPublic), CancellationToken.None);

        await _notificationRepository.DidNotReceive()
            .DeleteByIdsAsync(Arg.Any<List<short>>(), Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiDeletedEvent>(),
            Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        var comment = new be.Domain.Entities.Comment { Id = 10, UserId = 1, IdPublic = Guid.NewGuid() };
        _commentRepository.GetByIdPublicAsync(comment.IdPublic, Arg.Any<CancellationToken>()).Returns(comment);
        _commentRepository.GetNotiIdsByCommentIdAsync(comment.Id, Arg.Any<CancellationToken>())
            .Returns(new List<short>());
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new Exception("fail")));

        Func<Task> act = async () =>
            await _handler.Handle(new DeleteCommentCommand(comment.IdPublic), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("fail");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}