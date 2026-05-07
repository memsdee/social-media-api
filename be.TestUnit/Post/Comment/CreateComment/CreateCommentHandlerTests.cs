using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.Posts;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Post.Comment.CreateComment;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Tests.Post.Comment.CreateComment;

public class CreateCommentHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly CreateCommentHandler _handler;
    private readonly IFormat _helper;
    private readonly IMediator _mediator;
    private readonly INotiCmtRepository _notiCmtRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateCommentHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _commentRepository = Substitute.For<ICommentRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _notiCmtRepository = Substitute.For<INotiCmtRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _helper = Substitute.For<IFormat>();
        _mediator = Substitute.For<IMediator>();

        _handler = new CreateCommentHandler(
            _currentUserContext,
            _userRepository,
            _postRepository,
            _commentRepository,
            _notificationRepository,
            _notiCmtRepository,
            _outboxRepository,
            _unitOfWork,
            _helper,
            _mediator);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        _postRepository.GetPostForCommentAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PostForCommentDto?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new CreateCommentCommand { PostId = Guid.NewGuid(), Comment = "Hi" },
                CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Bài viết không tồn tại");
    }

    [Fact]
    public async Task Handle_WhenUserNotLoggedIn_ShouldThrowUnauthorizedException()
    {
        _postRepository.GetPostForCommentAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new PostForCommentDto
                { Id = 1, UserId = 2, PostAuthor = "author", Image = null, Content = "Post" });
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new CreateCommentCommand { PostId = Guid.NewGuid(), Comment = "Hi" },
                CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        var postId = Guid.NewGuid();
        _postRepository.GetPostForCommentAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new PostForCommentDto
                { Id = 1, UserId = 2, PostAuthor = "author", Image = null, Content = "Post" });
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetUser6Async("user-public-id", Arg.Any<CancellationToken>())
            .Returns((User6Dto?)null);

        Func<Task> act = async () => await _handler.Handle(new CreateCommentCommand { PostId = postId, Comment = "Hi" },
            CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenCommentOnOwnPost_ShouldSkipNotificationCreation()
    {
        var postId = Guid.NewGuid();
        var userPublicId = "user-public-id";
        var userPrivateId = (short)10;
        var post = new PostForCommentDto
            { Id = userPrivateId, UserId = userPrivateId, PostAuthor = "author", Image = null, Content = "Post" };
        var user = new User6Dto
            { PrivateUserId = userPrivateId, PublicUserId = userPublicId, Name = "Alice", Avatar = null };
        var transaction = Substitute.For<ITransaction>();

        _postRepository.GetPostForCommentAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _currentUserContext.UserId.Returns(userPublicId);
        _userRepository.GetUser6Async(userPublicId, Arg.Any<CancellationToken>()).Returns(user);
        _helper.FormatNotiPreview("Nice post").Returns("Nice post");
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _commentRepository.CountByPostIdAsync(userPrivateId, Arg.Any<CancellationToken>()).Returns(1);
        _helper.FormatImageUrl(null, userPublicId).Returns("avatar-url");

        var result = await _handler.Handle(new CreateCommentCommand { PostId = postId, Comment = "Nice post" },
            CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Comment.Should().Be("Nice post");
        result.Data.UserId.Should().Be(userPublicId);
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notifications>(), Arg.Any<CancellationToken>());
        await _notiCmtRepository.DidNotReceive().AddAsync(Arg.Any<NotiCmt>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.comment, Arg.Any<CommentCreatedEvent>(),
            Arg.Any<CancellationToken>());
        await transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommentOnOthersPost_ShouldCreateNotificationAndSignal()
    {
        var postId = Guid.NewGuid();
        var userPublicId = "user-public-id";
        var userPrivateId = (short)10;
        var postPrivateId = (short)20;
        var commentCount = 3;
        var post = new PostForCommentDto
        {
            Id = postPrivateId, UserId = postPrivateId, PostAuthor = "author", Image = Guid.NewGuid(), Content = "Post"
        };
        var user = new User6Dto
            { PrivateUserId = userPrivateId, PublicUserId = userPublicId, Name = "Alice", Avatar = Guid.NewGuid() };
        var transaction = Substitute.For<ITransaction>();

        _postRepository.GetPostForCommentAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _currentUserContext.UserId.Returns(userPublicId);
        _userRepository.GetUser6Async(userPublicId, Arg.Any<CancellationToken>()).Returns(user);
        _helper.FormatNotiPreview("Nice post").Returns("Nice post");
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _commentRepository.CountByPostIdAsync(postPrivateId, Arg.Any<CancellationToken>()).Returns(commentCount);
        _helper.FormatImageUrl(user.Avatar, userPublicId).Returns("avatar-url");

        await _handler.Handle(
            new CreateCommentCommand { PostId = postId, Comment = "Nice post", ConnectionId = "conn-1" },
            CancellationToken.None);

        await _notificationRepository.Received(1).AddAsync(Arg.Is<Notifications>(x =>
            x.SenderId == userPrivateId &&
            x.ReciverId == postPrivateId &&
            x.Target == NotiTargetEnum.post &&
            x.Action == NotiActionEnum.comment), Arg.Any<CancellationToken>());
        await _notiCmtRepository.Received(1).AddAsync(Arg.Is<NotiCmt>(x =>
            x.PostId == postPrivateId &&
            x.Preview == "Nice post"), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.notiComment, Arg.Any<NotiCommentEvent>(),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Any<Signal>(), Arg.Any<CancellationToken>());
        await transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollback()
    {
        var postId = Guid.NewGuid();
        var userPublicId = "user-public-id";
        var userPrivateId = (short)10;
        var postPrivateId = (short)20;
        var post = new PostForCommentDto
            { Id = postPrivateId, UserId = postPrivateId, PostAuthor = "author", Image = null, Content = "Post" };
        var user = new User6Dto
            { PrivateUserId = userPrivateId, PublicUserId = userPublicId, Name = "Alice", Avatar = null };
        var transaction = Substitute.For<ITransaction>();

        _postRepository.GetPostForCommentAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _currentUserContext.UserId.Returns(userPublicId);
        _userRepository.GetUser6Async(userPublicId, Arg.Any<CancellationToken>()).Returns(user);
        _helper.FormatNotiPreview("Nice post").Returns("Nice post");
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _commentRepository.CountByPostIdAsync(postPrivateId, Arg.Any<CancellationToken>()).Returns(1);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () =>
            await _handler.Handle(new CreateCommentCommand { PostId = postId, Comment = "Nice post" },
                CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}