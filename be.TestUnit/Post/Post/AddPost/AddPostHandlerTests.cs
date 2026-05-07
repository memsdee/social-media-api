using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Post.Post.AddPost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Post.AddPost;

public class AddPostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFormat _format;
    private readonly AddPostHandler _handler;
    private readonly IImage _image;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public AddPostHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _format = Substitute.For<IFormat>();
        _image = Substitute.For<IImage>();

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
    public async Task Handle_WhenUserNotLoggedIn_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new AddPostCommand { Content = "Hello" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUser2Async("user-id", Arg.Any<CancellationToken>())
            .Returns((User2Dto?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new AddPostCommand { Content = "Hello" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldCreatePostAndOutboxAndMoveImages()
    {
        var publicUserId = "user-id";
        var userSequenceId = (short)10;
        var user = new User2Dto { SequenceId = userSequenceId, Name = "Alice", Avatar = Guid.NewGuid() };
        var transaction = Substitute.For<ITransaction>();
        var imageId = Guid.NewGuid();
        var command = new AddPostCommand
        {
            Content = "Hello world",
            Images = [new ImageItem { Image = imageId, Type = ImageEnum.normal }]
        };

        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetUser2Async(publicUserId, Arg.Any<CancellationToken>()).Returns(user);
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("url");

        be.Domain.Entities.Post? addedPost = null;
        await _postRepository.AddAsync(Arg.Do<be.Domain.Entities.Post>(x => addedPost = x),
            Arg.Any<CancellationToken>());

        // Mock ID assignment after first SaveChanges
        _unitOfWork.When(x => x.SaveChangesAsync(Arg.Any<CancellationToken>()))
            .Do(call =>
            {
                if (addedPost != null) addedPost.Id = 100;
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Content.Should().Be("Hello world");

        await _postRepository.Received(1).AddAsync(Arg.Any<be.Domain.Entities.Post>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.post, Arg.Is<PostEvent>(e =>
            e.SequenceId == 100 &&
            e.Content == "Hello world" &&
            e.UserPublicId == publicUserId), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _image.Received(1)
            .MoveImageAsync(Arg.Is<List<Guid>>(l => l.Contains(imageId)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        var publicUserId = "user-id";
        _currentUserContext.UserId.Returns(publicUserId);
        _userRepository.GetUser2Async(publicUserId, Arg.Any<CancellationToken>())
            .Returns(new User2Dto { SequenceId = 1, Name = "Alice" });

        var transaction = Substitute.For<ITransaction>();
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(transaction);
        _postRepository.AddAsync(Arg.Any<be.Domain.Entities.Post>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () =>
            await _handler.Handle(new AddPostCommand { Content = "Hello" }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}