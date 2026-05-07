using be.Application.Common.Settings;
using be.Application.Features.Post.Post.GetPost;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Documents;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Post.GetPost;

public class GetPostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly DefaultInfoSettings _defaultInfo;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly GetPostHandler _handler;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IReactReadRepository _reactReadRepository;

    public GetPostHandlerTests()
    {
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _reactReadRepository = Substitute.For<IReactReadRepository>();
        _format = Substitute.For<IFormat>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _defaultInfo = new DefaultInfoSettings
        {
            Avatar = Guid.NewGuid(),
            DeletedName = "Deleted User",
            DeletedAvatar = Guid.NewGuid()
        };

        _handler = new GetPostHandler(
            _postReadRepository,
            _followReadRepository,
            _reactReadRepository,
            _format,
            _currentUserContext,
            _defaultInfo);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        var postId = Guid.NewGuid();
        _postReadRepository.GetByPublicIdAsync(postId, Arg.Any<CancellationToken>())
            .Returns((PostDocument?)null);

        Func<Task> act = async () => await _handler.Handle(new GetPostQuery(postId), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại bài viết");
    }

    [Fact]
    public async Task Handle_WhenPostNotActive_ShouldThrowNotFoundException()
    {
        var postId = Guid.NewGuid();
        _postReadRepository.GetByPublicIdAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new PostDocument { Status = StatusPostEnum.deleted });

        Func<Task> act = async () => await _handler.Handle(new GetPostQuery(postId), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Không tồn tại bài viết");
    }

    [Fact]
    public async Task Handle_WhenPostExists_ShouldReturnPostResponse()
    {
        var postId = Guid.NewGuid();
        var authorId = "author-1";
        var post = new PostDocument
        {
            IdPublic = postId,
            UserIdPublic = authorId,
            UserName = "Author One",
            UserAvatar = Guid.NewGuid(),
            Content = "Post content",
            Status = StatusPostEnum.active,
            CreateAt = DateTimeOffset.UtcNow,
            Images =
            [
                new PostImageReadModel { Image = Guid.NewGuid(), ImageType = ImageEnum.normal }
            ]
        };

        _postReadRepository.GetByPublicIdAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _currentUserContext.UserId.Returns("user-1");
        _followReadRepository.IsFollowingAsync("user-1", authorId, Arg.Any<CancellationToken>()).Returns(true);
        _reactReadRepository.GetReactAsync(postId, "user-1", Arg.Any<CancellationToken>()).Returns(ReactEnum.like);
        _reactReadRepository.GetReactAsync(postId, authorId, Arg.Any<CancellationToken>()).Returns(ReactEnum.dislike);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("url");

        var result = await _handler.Handle(new GetPostQuery(postId), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.IdPublic.Should().Be(postId);
        result.Data.PostAuthor.UserId.Should().Be(authorId);
        result.Data.PostAuthor.IsFollow.Should().BeTrue();
        result.Data.MyReact.Should().Be(ReactEnum.like);
        result.Data.IsReact.Should().Be(ReactEnum.dislike);
        result.Data.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenAccountIsDeleted_ShouldReturnDeletedInfo()
    {
        var postId = Guid.NewGuid();
        var authorId = "author-1";
        var post = new PostDocument
        {
            IdPublic = postId,
            UserIdPublic = authorId,
            UserName = "Author One",
            UserAvatar = Guid.NewGuid(),
            Content = "Post content",
            Status = StatusPostEnum.active,
            IsDeleteAccount = true,
            Images = []
        };

        _postReadRepository.GetByPublicIdAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _currentUserContext.UserId.Returns((string?)null); // Anonymous
        _reactReadRepository.GetReactAsync(postId, authorId, Arg.Any<CancellationToken>()).Returns((ReactEnum?)null);
        _format.FormatImageUrl(_defaultInfo.DeletedAvatar, authorId).Returns("deleted-avatar-url");

        var result = await _handler.Handle(new GetPostQuery(postId), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.PostAuthor.UserId.Should().BeNull();
        result.Data.PostAuthor.UserName.Should().Be(_defaultInfo.DeletedName);
        result.Data.PostAuthor.UserAvatar.Should().Be("deleted-avatar-url");
    }

    [Fact]
    public async Task Handle_WhenGroupedImages_ShouldReconstructGroups()
    {
        var postId = Guid.NewGuid();
        var authorId = "author-1";
        var beforeId = Guid.NewGuid();
        var afterId = Guid.NewGuid();
        var post = new PostDocument
        {
            IdPublic = postId,
            UserIdPublic = authorId,
            Status = StatusPostEnum.active,
            Images =
            [
                new PostImageReadModel { Image = beforeId, ImageType = ImageEnum.before, ImageGroupId = 1 },
                new PostImageReadModel { Image = afterId, ImageType = ImageEnum.after, ImageGroupId = 1 }
            ]
        };

        _postReadRepository.GetByPublicIdAsync(postId, Arg.Any<CancellationToken>()).Returns(post);
        _format.FormatImageUrl(beforeId, authorId).Returns("before-url");
        _format.FormatImageUrl(afterId, authorId).Returns("after-url");

        var result = await _handler.Handle(new GetPostQuery(postId), CancellationToken.None);

        result.Data!.Images.Should().HaveCount(1);
        result.Data.Images![0].Type.Should().Be(ImageEnum.before);
        result.Data.Images[0].Before.Should().Be("before-url");
        result.Data.Images[0].After.Should().Be("after-url");
        result.Data.Images[0].Image.Should().BeEmpty();
    }
}