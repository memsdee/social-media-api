using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Post.Post.AddPost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace be.Tests.Unit.Features.Post;

public class AddPostHandlerTests
{
    private static readonly string FakeUserId = "user-public-001";

    private static readonly User2Dto FakeUser = new()
    {
        SequenceId = 1,
        PublicUserId = FakeUserId,
        Name = "Test User",
        Avatar = Guid.NewGuid(),
        IsDeleteAccount = false
    };

    private readonly ICurrentUserContext _currentUserContext = Substitute.For<ICurrentUserContext>();
    private readonly IFormat _format = Substitute.For<IFormat>();

    private readonly AddPostHandler _handler;
    private readonly IImage _image = Substitute.For<IImage>();
    private readonly IOutboxRepository _outboxRepository = Substitute.For<IOutboxRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly ITransaction _transaction = Substitute.For<ITransaction>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    public AddPostHandlerTests()
    {
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(_transaction);

        _handler = new AddPostHandler(
            _currentUserContext,
            _userRepository,
            _postRepository,
            _outboxRepository,
            _unitOfWork,
            _format,
            _image);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsPostResponse()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>())
            .Returns("https://cdn.example.com/image.webp");

        var command = new AddPostCommand
        {
            Content = "Hello world",
            Images = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Equal("Hello world", result.Data.Content);
        Assert.Equal(FakeUserId, result.Data.PostAuthor.UserId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChangesAndCommitsTransaction()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);

        var command = new AddPostCommand { Content = "Test post", Images = [] };

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_InsertsOutboxRecord()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);

        var command = new AddPostCommand { Content = "Outbox test", Images = [] };

        await _handler.Handle(command, CancellationToken.None);

        await _outboxRepository.Received(1)
            .AddAsync(OutboxTopicEnum.post, Arg.Any<PostEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithImages_CallsMoveImageAsync()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);

        var imageId = Guid.NewGuid();
        var command = new AddPostCommand
        {
            Content = "Post with image",
            Images =
            [
                new ImageItem { Image = imageId, Type = ImageEnum.normal, GroupId = null }
            ]
        };

        await _handler.Handle(command, CancellationToken.None);

        await _image.Received(1)
            .MoveImageAsync(Arg.Is<List<Guid>>(l => l.Contains(imageId)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoImages_DoesNotCallMoveImageAsync()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);

        var command = new AddPostCommand { Content = "No image post", Images = [] };

        await _handler.Handle(command, CancellationToken.None);

        await _image.DidNotReceive()
            .MoveImageAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserIdNull_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        var command = new AddPostCommand { Content = "Test", Images = [] };

        await Assert.ThrowsAsync<CustomException.UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns((User2Dto?)null);

        var command = new AddPostCommand { Content = "Test", Images = [] };

        await Assert.ThrowsAsync<CustomException.UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SaveChangesFails_RollsBackTransaction()
    {
        _currentUserContext.UserId.Returns(FakeUserId);
        _userRepository.GetUser2Async(FakeUserId, Arg.Any<CancellationToken>())
            .Returns(FakeUser);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("DB error"));

        var command = new AddPostCommand { Content = "Test", Images = [] };

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}