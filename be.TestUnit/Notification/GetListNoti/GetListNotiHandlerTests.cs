using System.Text.Json;
using be.Application.Dtos.Notification;
using be.Application.Dtos.Pagination;
using be.Application.Features.Notification.GetListNoti;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Documents;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Notification.GetListNoti;

public class GetListNotiHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly IFormat _format;
    private readonly GetListNotiHandler _handler;
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly IUserRepository _userRepository;

    public GetListNotiHandlerTests()
    {
        _format = Substitute.For<IFormat>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _encryption = Substitute.For<IEncryption>();
        _notificationReadRepository = Substitute.For<INotificationReadRepository>();

        _handler = new GetListNotiHandler(
            _format,
            _currentUserContext,
            _userRepository,
            _encryption,
            _notificationReadRepository);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new GetListNotiQuery { Limit = 10 }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetPrivateIdByPublicIdAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () => await _handler.Handle(new GetListNotiQuery { Limit = 10 }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenNoNotifications_ShouldReturnEmptyList()
    {
        var myPublicId = "user-public-id";
        var myPrivateId = (short)11;
        _currentUserContext.UserId.Returns(myPublicId);
        _userRepository.GetPrivateIdByPublicIdAsync(myPublicId, Arg.Any<CancellationToken>()).Returns(myPrivateId);
        _notificationReadRepository.GetByReceiverAsync(myPrivateId, null, null, 50, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationDocument>());
        _notificationReadRepository.GetCommentUniqueSenderCountsAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, int>());
        _notificationReadRepository.GetFollowUniqueSenderCountAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(0);
        _notificationReadRepository
            .GetUsersBySequenceIdsAsync(Arg.Any<IReadOnlyCollection<short>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, UserDocument>());

        var result = await _handler.Handle(new GetListNotiQuery { Limit = 10 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Notifications.Should().BeEmpty();
        result.Data.PageProfile.HasNextPage.Should().BeFalse();
        result.Data.PageProfile.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenWithCursor_ShouldDecryptAndPassCursorToRepository()
    {
        var myPublicId = "user-public-id";
        var myPrivateId = (short)11;
        var cursorPayload = new CursorPayload<DateTimeOffset>(DateTimeOffset.Parse("2026-04-28T10:00:00Z"), 42);
        var encryptedCursor = "encrypted-cursor";
        var decryptedCursorJson = JsonSerializer.Serialize(cursorPayload);

        _currentUserContext.UserId.Returns(myPublicId);
        _userRepository.GetPrivateIdByPublicIdAsync(myPublicId, Arg.Any<CancellationToken>()).Returns(myPrivateId);
        _encryption.Decrypt(encryptedCursor).Returns(decryptedCursorJson);
        _notificationReadRepository.GetByReceiverAsync(myPrivateId, cursorPayload.Selector, cursorPayload.Id, 5,
                Arg.Any<CancellationToken>())
            .Returns(new List<NotificationDocument>());
        _notificationReadRepository.GetCommentUniqueSenderCountsAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, int>());
        _notificationReadRepository.GetFollowUniqueSenderCountAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(0);
        _notificationReadRepository
            .GetUsersBySequenceIdsAsync(Arg.Any<IReadOnlyCollection<short>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, UserDocument>());

        await _handler.Handle(new GetListNotiQuery { Limit = 1, Cursor = encryptedCursor }, CancellationToken.None);

        _encryption.Received(1).Decrypt(encryptedCursor);
        await _notificationReadRepository.Received(1).GetByReceiverAsync(myPrivateId, cursorPayload.Selector,
            cursorPayload.Id, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHasNextPage_ShouldReturnEncryptedNextCursor()
    {
        var myPublicId = "user-public-id";
        var myPrivateId = (short)11;
        var sender1Avatar = Guid.NewGuid();
        var sender2Avatar = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        _currentUserContext.UserId.Returns(myPublicId);
        _userRepository.GetPrivateIdByPublicIdAsync(myPublicId, Arg.Any<CancellationToken>()).Returns(myPrivateId);

        var notifications = new List<NotificationDocument>
        {
            new()
            {
                SequenceId = 2,
                ReceiveSequenceId = myPrivateId,
                SenderSequenceId = 20,
                Target = NotiTargetEnum.user,
                Action = NotiActionEnum.follow,
                CreateAt = now,
                ReadAt = null
            },
            new()
            {
                SequenceId = 1,
                ReceiveSequenceId = myPrivateId,
                SenderSequenceId = 30,
                Target = NotiTargetEnum.post,
                Action = NotiActionEnum.comment,
                PostPublicId = Guid.NewGuid(),
                CmtPublicId = Guid.NewGuid(),
                PreviewContent = "Nice post",
                CreateAt = now.AddMinutes(-5),
                ReadAt = null
            }
        };

        _notificationReadRepository.GetByReceiverAsync(myPrivateId, null, null, 5, Arg.Any<CancellationToken>())
            .Returns(notifications);
        _notificationReadRepository.GetCommentUniqueSenderCountsAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, int> { { 1, 1 } });
        _notificationReadRepository.GetFollowUniqueSenderCountAsync(myPrivateId, Arg.Any<CancellationToken>())
            .Returns(1);
        _notificationReadRepository.GetUsersBySequenceIdsAsync(
                Arg.Is<IReadOnlyCollection<short>>(ids =>
                    ids.Count == 2 && ids.Contains((short)20) && ids.Contains((short)30)), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, UserDocument>
            {
                {
                    20,
                    new UserDocument
                        { SequenceId = 20, Name = "Alice", Avatar = sender1Avatar, UserIdPublic = "alice-public" }
                },
                {
                    30,
                    new UserDocument
                        { SequenceId = 30, Name = "Bob", Avatar = sender2Avatar, UserIdPublic = "bob-public" }
                }
            });
        _format.FormatImageUrl(sender1Avatar, "Alice").Returns("alice-avatar");
        _format.FormatImageUrl(sender2Avatar, "Bob").Returns("bob-avatar");
        _encryption.Encrypt(Arg.Any<string>()).Returns("enc-next-cursor");

        var result = await _handler.Handle(new GetListNotiQuery { Limit = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Notifications.Should().HaveCount(1);
        result.Data.Notifications[0].Should().BeOfType<UserFollowNotificationDto>();
        result.Data.PageProfile.HasNextPage.Should().BeTrue();
        result.Data.PageProfile.NextCursor.Should().Be("enc-next-cursor");
    }
}