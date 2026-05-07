using System.Text.Json;
using be.Application.Common.Settings;
using be.Application.Dtos.Pagination;
using be.Application.Features.Post.Comment.GetListComment;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Documents;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Comment.GetListComment;

public class GetListCommentHandlerTests
{
    private readonly ICommentReadRepository _commentReadRepository;
    private readonly DefaultInfoSettings _defaultInfo;
    private readonly IEncryption _encryption;
    private readonly IFormat _format;
    private readonly GetListCommentHandler _handler;
    private readonly IPostReadRepository _postReadRepository;

    public GetListCommentHandlerTests()
    {
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _commentReadRepository = Substitute.For<ICommentReadRepository>();
        _encryption = Substitute.For<IEncryption>();
        _format = Substitute.For<IFormat>();

        _defaultInfo = new DefaultInfoSettings
        {
            DeletedName = "[deleted]",
            DeletedAvatar = Guid.NewGuid()
        };

        _handler = new GetListCommentHandler(
            _postReadRepository,
            _commentReadRepository,
            _encryption,
            _format,
            _defaultInfo);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        _postReadRepository.GetByPublicIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PostDocument?)null);

        Func<Task> act = async () => await _handler.Handle(new GetListCommentQuery
        {
            TargetId = Guid.NewGuid(),
            Limit = 10
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại bài đăng");
    }

    [Fact]
    public async Task Handle_WhenNoCursor_ShouldReturnMappedCommentsAndNoNextCursor()
    {
        var post = new PostDocument { SequenceId = 22, IdPublic = Guid.NewGuid() };
        var comment = new CommentDocument
        {
            SequenceId = 5,
            Content = "hello",
            IdPublic = Guid.NewGuid(),
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "user_public",
            UserName = "User",
            UserAvatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        var cursorResult = new CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>(
            new List<CommentDocument> { comment },
            false,
            null);

        _postReadRepository.GetByPublicIdAsync(post.IdPublic, Arg.Any<CancellationToken>())
            .Returns(post);
        _commentReadRepository
            .GetPagedByPostSequenceIdAsync(post.SequenceId, 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("http://avatar.url");

        var result = await _handler.Handle(new GetListCommentQuery
        {
            TargetId = post.IdPublic,
            Limit = 10,
            Cursor = null
        }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Comments.Should().HaveCount(1);

        var c = result.Data.Comments[0];
        c.Comment.Should().Be(comment.Content);
        c.IdPublic.Should().Be(comment.IdPublic);
        c.CreatedAt.Should().Be(comment.CreateAt);
        c.UserId.Should().Be(comment.UserIdPublic);
        c.UserName.Should().Be(comment.UserName);
        c.UserAvatar.Should().Be("http://avatar.url");

        result.Data.PageProfile.HasNextPage.Should().BeFalse();
        result.Data.PageProfile.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenHasNextCursor_ShouldReturnEncryptedNextCursor()
    {
        var post = new PostDocument { SequenceId = 22, IdPublic = Guid.NewGuid() };
        var comment = new CommentDocument
        {
            SequenceId = 7,
            Content = "more",
            IdPublic = Guid.NewGuid(),
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "u2",
            UserName = "Someone",
            UserAvatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        var nextCursor = new CursorPayload<DateTimeOffset>(DateTimeOffset.UtcNow, 2);
        var cursorResult = new CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>(
            new List<CommentDocument> { comment },
            true,
            nextCursor);

        _postReadRepository.GetByPublicIdAsync(post.IdPublic, Arg.Any<CancellationToken>())
            .Returns(post);
        _commentReadRepository
            .GetPagedByPostSequenceIdAsync(post.SequenceId, 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted-cursor");
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("http://avatar2.url");

        var result = await _handler.Handle(new GetListCommentQuery
        {
            TargetId = post.IdPublic,
            Limit = 10,
            Cursor = null
        }, CancellationToken.None);

        result.Data!.PageProfile.HasNextPage.Should().BeTrue();
        result.Data.PageProfile.NextCursor.Should().Be("encrypted-cursor");
    }

    [Fact]
    public async Task Handle_WhenIsDeleteAccount_ShouldAnonymizeUser()
    {
        var post = new PostDocument { SequenceId = 22, IdPublic = Guid.NewGuid() };
        var comment = new CommentDocument
        {
            SequenceId = 3,
            Content = "deleted comment",
            IdPublic = Guid.NewGuid(),
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "u_del",
            UserName = "ShouldBeHidden",
            UserAvatar = Guid.NewGuid(),
            IsDeleteAccount = true
        };

        var cursorResult = new CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>(
            new List<CommentDocument> { comment },
            false,
            null);

        _postReadRepository.GetByPublicIdAsync(post.IdPublic, Arg.Any<CancellationToken>())
            .Returns(post);
        _commentReadRepository
            .GetPagedByPostSequenceIdAsync(post.SequenceId, 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("deleted-avatar-url");

        var result = await _handler.Handle(new GetListCommentQuery
        {
            TargetId = post.IdPublic,
            Limit = 10,
            Cursor = null
        }, CancellationToken.None);

        var c = result.Data!.Comments[0];
        c.UserId.Should().BeNull();
        c.UserName.Should().Be(_defaultInfo.DeletedName);
        c.UserAvatar.Should().Be("deleted-avatar-url");
    }

    [Fact]
    public async Task Handle_WhenCursorProvided_ShouldDecryptAndDeserialize()
    {
        var post = new PostDocument { SequenceId = 22, IdPublic = Guid.NewGuid() };
        var expectedSelector = new DateTimeOffset(2026, 4, 27, 12, 0, 0, TimeSpan.Zero);
        var expectedCursor = new CursorPayload<DateTimeOffset>(expectedSelector, 99);
        var serialized = JsonSerializer.Serialize(expectedCursor);

        _postReadRepository.GetByPublicIdAsync(post.IdPublic, Arg.Any<CancellationToken>())
            .Returns(post);
        _encryption.Decrypt("encrypted-input").Returns(serialized);

        var comment = new CommentDocument
        {
            SequenceId = 1,
            Content = "c",
            IdPublic = Guid.NewGuid(),
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "u",
            UserName = "n",
            UserAvatar = Guid.NewGuid(),
            IsDeleteAccount = false
        };

        var cursorResult = new CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>(
            new List<CommentDocument> { comment },
            false,
            null);

        _commentReadRepository
            .GetPagedByPostSequenceIdAsync(Arg.Any<short>(), Arg.Any<int>(), Arg.Any<CursorPayload<DateTimeOffset>?>(),
                Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("url");

        await _handler.Handle(new GetListCommentQuery
        {
            TargetId = post.IdPublic,
            Limit = 10,
            Cursor = "encrypted-input"
        }, CancellationToken.None);

        await _commentReadRepository.Received(1).GetPagedByPostSequenceIdAsync(
            post.SequenceId,
            10,
            Arg.Is<CursorPayload<DateTimeOffset>?>(c =>
                c != null && c.Selector == expectedSelector && c.Id == expectedCursor.Id),
            Arg.Any<CancellationToken>());
    }
}