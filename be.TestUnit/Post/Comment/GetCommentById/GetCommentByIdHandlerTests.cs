using be.Application.Common.Settings;
using be.Application.Features.Post.Comment.GetCommentById;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Documents;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Comment.GetCommentById;

public class GetCommentByIdHandlerTests
{
    private readonly ICommentReadRepository _commentReadRepository;
    private readonly DefaultInfoSettings _defaultInfo;
    private readonly IFormat _format;
    private readonly GetCommentByIdHandler _handler;

    public GetCommentByIdHandlerTests()
    {
        _commentReadRepository = Substitute.For<ICommentReadRepository>();
        _format = Substitute.For<IFormat>();
        _defaultInfo = new DefaultInfoSettings
        {
            Avatar = Guid.NewGuid(),
            DeletedName = "Deleted User",
            DeletedAvatar = Guid.NewGuid()
        };

        _handler = new GetCommentByIdHandler(
            _commentReadRepository,
            _format,
            _defaultInfo);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        var commentId = Guid.NewGuid();
        _commentReadRepository.GetByIdPublicAsync(commentId, Arg.Any<CancellationToken>())
            .Returns((CommentDocument?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new GetCommentByIdQuery { CommentId = commentId }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại bình luận");
    }

    [Fact]
    public async Task Handle_WhenCommentExists_ShouldReturnCommentResponse()
    {
        var commentId = Guid.NewGuid();
        var postSequenceId = (short)1;
        var comment = new CommentDocument
        {
            IdPublic = commentId,
            Content = "Test comment",
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "user-1",
            UserName = "User One",
            UserAvatar = Guid.NewGuid(),
            PostSquenceId = postSequenceId,
            IsDeleteAccount = false
        };

        _commentReadRepository.GetByIdPublicAsync(commentId, Arg.Any<CancellationToken>())
            .Returns(comment);
        _commentReadRepository.CountByPostSequenceIdAsync(postSequenceId, Arg.Any<CancellationToken>())
            .Returns(5);
        _format.FormatImageUrl(comment.UserAvatar, comment.UserIdPublic).Returns("avatar-url");

        var result = await _handler.Handle(new GetCommentByIdQuery { CommentId = commentId }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Comment.Should().Be(comment.Content);
        result.Data.IdPublic.Should().Be(commentId);
        result.Data.UserId.Should().Be(comment.UserIdPublic);
        result.Data.UserName.Should().Be(comment.UserName);
        result.Data.UserAvatar.Should().Be("avatar-url");
        result.Data.TotalComments.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenAccountIsDeleted_ShouldReturnDeletedInfo()
    {
        var commentId = Guid.NewGuid();
        var postSequenceId = (short)1;
        var comment = new CommentDocument
        {
            IdPublic = commentId,
            Content = "Test comment",
            CreateAt = DateTimeOffset.UtcNow,
            UserIdPublic = "user-1",
            UserName = "User One",
            UserAvatar = Guid.NewGuid(),
            PostSquenceId = postSequenceId,
            IsDeleteAccount = true
        };

        _commentReadRepository.GetByIdPublicAsync(commentId, Arg.Any<CancellationToken>())
            .Returns(comment);
        _commentReadRepository.CountByPostSequenceIdAsync(postSequenceId, Arg.Any<CancellationToken>())
            .Returns(5);
        _format.FormatImageUrl(_defaultInfo.DeletedAvatar, comment.UserIdPublic).Returns("deleted-avatar-url");

        var result = await _handler.Handle(new GetCommentByIdQuery { CommentId = commentId }, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().BeNull();
        result.Data.UserName.Should().Be(_defaultInfo.DeletedName);
        result.Data.UserAvatar.Should().Be("deleted-avatar-url");
    }
}