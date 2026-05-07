using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.Post.React.LikePost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Tests.Post.React.LikePost;

public class LikePostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFormat _format;
    private readonly LikePostHandler _handler;
    private readonly IMediator _mediator;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotiReactPostRepository _notiReactPostRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly IReactPostRepository _reactPostRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public LikePostHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _reactPostRepository = Substitute.For<IReactPostRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _notiReactPostRepository = Substitute.For<INotiReactPostRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _format = Substitute.For<IFormat>();
        _mediator = Substitute.For<IMediator>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mediator.Publish(Arg.Any<Signal>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        _handler = new LikePostHandler(
            _currentUserContext,
            _userRepository,
            _postRepository,
            _reactPostRepository,
            _notificationRepository,
            _notiReactPostRepository,
            _outboxRepository,
            _unitOfWork,
            _format,
            _mediator);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new LikePostCommand { PostId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new LikePostCommand { PostId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short)7);
        _postRepository.GetPostForReactAsync(Arg.Any<Guid>(), 7, Arg.Any<CancellationToken>())
            .Returns((PostForReactDto?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new LikePostCommand { PostId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoExistingReactAndNotOwnPost_ShouldCreateNotification()
    {
        var postId = Guid.NewGuid();
        var privateUserId = (short)1;
        var post = new PostForReactDto
        {
            Id = 10,
            UserId = 2,
            PostAuthorPublicId = "author",
            Thumbnail = Guid.NewGuid(),
            Content = "hello",
            AuthorReact = null
        };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postId, privateUserId, Arg.Any<CancellationToken>())
            .Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, post.Id, Arg.Any<CancellationToken>())
            .Returns((ReactPost?)null);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new List<short>());
        _notificationRepository
            .GetBySenderAndIdsAsync(privateUserId, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns((Notifications?)null);
        _reactPostRepository.AddAsync(Arg.Any<ReactPost>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _notificationRepository.AddAsync(Arg.Any<Notifications>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notiReactPostRepository.AddAsync(Arg.Any<NotiReactPost>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository.AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<ReactPostEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository
            .AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<NotiReactPostEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _format.FormatNotiPreview(Arg.Any<string?>()).Returns("preview");
        _reactPostRepository.GetTotalReactsAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short> { { ReactEnum.like, 2 }, { ReactEnum.dislike, 1 } });

        var result = await _handler.Handle(new LikePostCommand { PostId = postId }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.MyReact.Should().Be(ReactEnum.like);
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notifications>(), Arg.Any<CancellationToken>());
        await _notiReactPostRepository.Received(1).AddAsync(Arg.Any<NotiReactPost>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiReactPostEvent>(),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reactPost, Arg.Any<ReactPostEvent>(),
            Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingLike_ShouldDeleteReactAndNotification()
    {
        var postId = Guid.NewGuid();
        var privateUserId = (short)1;
        var post = new PostForReactDto { Id = 10, UserId = 2, PostAuthorPublicId = "author" };
        var existingReact = new ReactPost { Id = 33, PostId = 10, UserId = privateUserId, Type = ReactEnum.like };
        var existingNoti = new Notifications { Id = 9, CreatedAt = DateTimeOffset.UtcNow };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postId, privateUserId, Arg.Any<CancellationToken>())
            .Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, post.Id, Arg.Any<CancellationToken>())
            .Returns(existingReact);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new List<short> { 9 });
        _notificationRepository
            .GetBySenderAndIdsAsync(privateUserId, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns(existingNoti);
        _outboxRepository.AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<ReactPostEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository
            .AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<NotiDeletedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _reactPostRepository.GetTotalReactsAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short>());

        var result = await _handler.Handle(new LikePostCommand { PostId = postId }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.MyReact.Should().BeNull();
        _reactPostRepository.Received(1).Delete(existingReact);
        _notificationRepository.Received(1).Delete(existingNoti);
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reactPost, Arg.Any<ReactPostEvent>(),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiDeletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingDislike_ShouldSwitchToLikeAndEmitNotification()
    {
        var postId = Guid.NewGuid();
        var privateUserId = (short)1;
        var post = new PostForReactDto { Id = 10, UserId = 2, PostAuthorPublicId = "author" };
        var existingReact = new ReactPost { Id = 33, PostId = 10, UserId = privateUserId, Type = ReactEnum.dislike };
        var existingNoti = new Notifications { Id = 9, CreatedAt = DateTimeOffset.UtcNow };
        var notiReact = new NotiReactPost { NotiId = 9, PostId = 10, Type = ReactEnum.dislike };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postId, privateUserId, Arg.Any<CancellationToken>())
            .Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, post.Id, Arg.Any<CancellationToken>())
            .Returns(existingReact);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new List<short> { 9 });
        _notificationRepository
            .GetBySenderAndIdsAsync(privateUserId, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns(existingNoti);
        _notiReactPostRepository.GetByIdAsync(existingNoti.Id, Arg.Any<CancellationToken>())
            .Returns(notiReact);
        _outboxRepository.AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<ReactPostEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _outboxRepository
            .AddAsync(Arg.Any<OutboxTopicEnum>(), Arg.Any<NotiReactPostEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _reactPostRepository.GetTotalReactsAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short>());

        var result = await _handler.Handle(new LikePostCommand { PostId = postId }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.MyReact.Should().Be(ReactEnum.like);
        notiReact.Type.Should().Be(ReactEnum.like);
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.notification, Arg.Any<NotiReactPostEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        var postId = Guid.NewGuid();
        var privateUserId = (short)1;
        var post = new PostForReactDto { Id = 10, UserId = 2, PostAuthorPublicId = "author" };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postId, privateUserId, Arg.Any<CancellationToken>())
            .Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, post.Id, Arg.Any<CancellationToken>())
            .Returns((ReactPost?)null);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new List<short>());
        _notificationRepository
            .GetBySenderAndIdsAsync(privateUserId, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns((Notifications?)null);
        _reactPostRepository.AddAsync(Arg.Any<ReactPost>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () =>
            await _handler.Handle(new LikePostCommand { PostId = postId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}