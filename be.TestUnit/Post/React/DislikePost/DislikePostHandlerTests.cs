using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.Post.React.DislikePost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Tests.Post.React.DislikePost;

public class DislikePostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFormat _format;
    private readonly DislikePostHandler _handler;
    private readonly IMediator _mediator;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotiReactPostRepository _notiReactPostRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly IReactPostRepository _reactPostRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public DislikePostHandlerTests()
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

        _handler = new DislikePostHandler(
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
    public async Task Handle_WhenUserNotLoggedIn_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DislikePostCommand { PostId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _postRepository.GetPostForReactAsync(Arg.Any<Guid>(), Arg.Any<short>(), Arg.Any<CancellationToken>())
            .Returns((PostForReactDto?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new DislikePostCommand { PostId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoExistingReact_ShouldCreateNewDislike()
    {
        var publicUserId = "user-id";
        var privateUserId = (short)1;
        var postIdPublic = Guid.NewGuid();
        var postIdPrivate = (short)10;
        var authorIdPrivate = (short)20;
        var post = new PostForReactDto
            { Id = postIdPrivate, UserId = authorIdPrivate, PostAuthorPublicId = "author-id", Content = "Hello" };
        var transaction = Substitute.For<ITransaction>();

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetPrivateIdByPublicIdAsync(publicUserId, Arg.Any<CancellationToken>()).Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postIdPublic, privateUserId, Arg.Any<CancellationToken>()).Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, postIdPrivate, Arg.Any<CancellationToken>())
            .Returns((ReactPost?)null);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(postIdPrivate, Arg.Any<CancellationToken>()).Returns([]);
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _reactPostRepository.GetTotalReactsAsync(postIdPrivate, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short> { { ReactEnum.dislike, 1 } });

        var result = await _handler.Handle(new DislikePostCommand { PostId = postIdPublic }, CancellationToken.None);

        result.Data.MyReact.Should().Be(ReactEnum.dislike);
        await _reactPostRepository.Received(1).AddAsync(Arg.Is<ReactPost>(x => x.Type == ReactEnum.dislike),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reactPost,
            Arg.Is<ReactPostEvent>(x => x.Type == ReactEnum.dislike && !x.IsDelete), Arg.Any<CancellationToken>());
        await transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingDislike_ShouldDeleteReact()
    {
        var publicUserId = "user-id";
        var privateUserId = (short)1;
        var postIdPublic = Guid.NewGuid();
        var postIdPrivate = (short)10;
        var post = new PostForReactDto { Id = postIdPrivate, UserId = 20, PostAuthorPublicId = "author-id" };
        var existingReact = new ReactPost
            { Id = 100, PostId = postIdPrivate, UserId = privateUserId, Type = ReactEnum.dislike };
        var transaction = Substitute.For<ITransaction>();

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetPrivateIdByPublicIdAsync(publicUserId, Arg.Any<CancellationToken>()).Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postIdPublic, privateUserId, Arg.Any<CancellationToken>()).Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, postIdPrivate, Arg.Any<CancellationToken>())
            .Returns(existingReact);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(postIdPrivate, Arg.Any<CancellationToken>()).Returns([]);
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _reactPostRepository.GetTotalReactsAsync(postIdPrivate, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short>());

        var result = await _handler.Handle(new DislikePostCommand { PostId = postIdPublic }, CancellationToken.None);

        result.Data.MyReact.Should().BeNull();
        _reactPostRepository.Received(1).Delete(existingReact);
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reactPost, Arg.Is<ReactPostEvent>(x => x.IsDelete),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingLike_ShouldSwitchToDislike()
    {
        var publicUserId = "user-id";
        var privateUserId = (short)1;
        var postIdPublic = Guid.NewGuid();
        var postIdPrivate = (short)10;
        var post = new PostForReactDto { Id = postIdPrivate, UserId = 20, PostAuthorPublicId = "author-id" };
        var existingReact = new ReactPost
            { Id = 100, PostId = postIdPrivate, UserId = privateUserId, Type = ReactEnum.like };
        var transaction = Substitute.For<ITransaction>();

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetPrivateIdByPublicIdAsync(publicUserId, Arg.Any<CancellationToken>()).Returns(privateUserId);
        _postRepository.GetPostForReactAsync(postIdPublic, privateUserId, Arg.Any<CancellationToken>()).Returns(post);
        _reactPostRepository.GetByUserAndPostAsync(privateUserId, postIdPrivate, Arg.Any<CancellationToken>())
            .Returns(existingReact);
        _notiReactPostRepository.GetNotiIdsByPostIdAsync(postIdPrivate, Arg.Any<CancellationToken>()).Returns([]);
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _reactPostRepository.GetTotalReactsAsync(postIdPrivate, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactEnum, short> { { ReactEnum.dislike, 1 } });

        var result = await _handler.Handle(new DislikePostCommand { PostId = postIdPublic }, CancellationToken.None);

        result.Data.MyReact.Should().Be(ReactEnum.dislike);
        existingReact.Type.Should().Be(ReactEnum.dislike);
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reactPost,
            Arg.Is<ReactPostEvent>(x => x.Type == ReactEnum.dislike && !x.IsDelete), Arg.Any<CancellationToken>());
    }
}