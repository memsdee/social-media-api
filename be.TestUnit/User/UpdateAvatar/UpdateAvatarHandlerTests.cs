using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.User.UpdateAvatar;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.UpdateAvatar;

public class UpdateAvatarHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFormat _format;
    private readonly UpdateAvatarHandler _handler;
    private readonly IImage _image;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UpdateAvatarHandlerTests()
    {
        _image = Substitute.For<IImage>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _format = Substitute.For<IFormat>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _image.MoveImageAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _image.DeleteImageAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _transaction.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _transaction.RollbackAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        _outboxRepository.AddAsync(OutboxTopicEnum.updateAvatar, Arg.Any<UpdateAvatarEvent>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _handler = new UpdateAvatarHandler(
            _image,
            _currentUserContext,
            _userRepository,
            _format,
            _unitOfWork,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new UpdateAvatarCommand { AvatarId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
        await _userRepository.DidNotReceive().GetUser5Async(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("me");
        _userRepository.GetUser5Async("me", Arg.Any<CancellationToken>()).Returns((User5Dto?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new UpdateAvatarCommand { AvatarId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
        await _image.DidNotReceive().MoveImageAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHasOldAvatar_ShouldMoveDeleteUpdateAndReturnUrl()
    {
        var oldAvatar = Guid.NewGuid();
        var newAvatar = Guid.NewGuid();
        var user = new User5Dto
        {
            PrivateUserId = 10,
            PublicUserId = "me",
            Avatar = oldAvatar
        };

        _currentUserContext.UserId.Returns("me");
        _userRepository.GetUser5Async("me", Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.DeleteAvatarAsync(10, newAvatar, Arg.Any<CancellationToken>()).Returns(1);
        _format.FormatImageUrl(newAvatar, "me").Returns("avatar-url");

        var result = await _handler.Handle(new UpdateAvatarCommand { AvatarId = newAvatar }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.AvatarUrl.Should().Be("avatar-url");

        await _image.Received(1).MoveImageAsync(Arg.Is<List<Guid>>(x => x.Count == 1 && x[0] == newAvatar),
            Arg.Any<CancellationToken>());
        await _image.Received(1).DeleteImageAsync(Arg.Is<List<Guid>>(x => x.Count == 1 && x[0] == oldAvatar),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(
            OutboxTopicEnum.updateAvatar,
            Arg.Is<UpdateAvatarEvent>(e => e.PrivateUserId == 10 && e.Avatar == newAvatar),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoOldAvatar_ShouldNotDeleteOldAvatar()
    {
        var newAvatar = Guid.NewGuid();
        var user = new User5Dto
        {
            PrivateUserId = 10,
            PublicUserId = "me",
            Avatar = null
        };

        _currentUserContext.UserId.Returns("me");
        _userRepository.GetUser5Async("me", Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.DeleteAvatarAsync(10, newAvatar, Arg.Any<CancellationToken>()).Returns(1);
        _format.FormatImageUrl(newAvatar, "me").Returns("avatar-url");

        var result = await _handler.Handle(new UpdateAvatarCommand { AvatarId = newAvatar }, CancellationToken.None);

        result.Data!.AvatarUrl.Should().Be("avatar-url");
        await _image.DidNotReceive().DeleteImageAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateReturnsZero_ShouldRollbackAndThrow()
    {
        var newAvatar = Guid.NewGuid();
        var user = new User5Dto
        {
            PrivateUserId = 10,
            PublicUserId = "me",
            Avatar = Guid.NewGuid()
        };

        _currentUserContext.UserId.Returns("me");
        _userRepository.GetUser5Async("me", Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.DeleteAvatarAsync(10, newAvatar, Arg.Any<CancellationToken>()).Returns(0);

        Func<Task> act = async () =>
            await _handler.Handle(new UpdateAvatarCommand { AvatarId = newAvatar }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().AddAsync(OutboxTopicEnum.updateAvatar, Arg.Any<UpdateAvatarEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldRollback()
    {
        var newAvatar = Guid.NewGuid();
        var user = new User5Dto
        {
            PrivateUserId = 10,
            PublicUserId = "me",
            Avatar = Guid.NewGuid()
        };

        _currentUserContext.UserId.Returns("me");
        _userRepository.GetUser5Async("me", Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.DeleteAvatarAsync(10, newAvatar, Arg.Any<CancellationToken>()).Returns(1);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () =>
            await _handler.Handle(new UpdateAvatarCommand { AvatarId = newAvatar }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}