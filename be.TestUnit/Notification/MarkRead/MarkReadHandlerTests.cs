using System.Text.Json;
using be.Application.Dtos.EventBus;
using be.Application.Features.Notification.MarkRead;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Notification.MarkRead;

public class MarkReadHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly MarkReadHandler _handler;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public MarkReadHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _notificationRepository = Substitute.For<INotificationRepository>();
        _encryption = Substitute.For<IEncryption>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new MarkReadHandler(
            _currentUserContext,
            _userRepository,
            _notificationRepository,
            _encryption,
            _outboxRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new MarkReadCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new MarkReadCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenDecryptionFails_ShouldThrowBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _encryption.Decrypt(Arg.Any<string>()).Returns("invalid-json");

        Func<Task> act = async () =>
            await _handler.Handle(new MarkReadCommand { EncryptedIds = "some-ids" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Dữ liệu không hợp lệ");
    }

    [Fact]
    public async Task Handle_WhenIdsEmpty_ShouldThrowBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _encryption.Decrypt("encrypted-empty").Returns("[]");

        Func<Task> act = async () =>
            await _handler.Handle(new MarkReadCommand { EncryptedIds = "encrypted-empty" }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Dữ liệu không được để trống");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldCommitTransaction()
    {
        // Arrange
        var myPublicId = "public-id";
        var myPrivateId = (short)1;
        var ids = new short[] { 10, 11 };
        var encryptedIds = "encrypted-ids";

        _currentUserContext.UserId.Returns(myPublicId);
        _userRepository.GetPrivateIdByPublicIdAsync(myPublicId, Arg.Any<CancellationToken>()).Returns(myPrivateId);
        _encryption.Decrypt(encryptedIds).Returns(JsonSerializer.Serialize(ids));

        // Act
        var result = await _handler.Handle(new MarkReadCommand { EncryptedIds = encryptedIds }, CancellationToken.None);

        // Assert
        result.Message.Should().Be("Đã đánh dấu đã đọc");
        await _notificationRepository.Received(1).MarkReadAsync(Arg.Is<short[]>(x => x.Length == 2), myPrivateId,
            Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.markAsReadNotification,
            Arg.Any<MarkReadNotiEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenException_ShouldRollback()
    {
        // Arrange
        _currentUserContext.UserId.Returns("public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("public-id", Arg.Any<CancellationToken>()).Returns((short)1);
        _encryption.Decrypt("ids").Returns("[1]");

        _unitOfWork.When(x => x.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(_ => throw new Exception("fail"));

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(new MarkReadCommand { EncryptedIds = "ids" }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}