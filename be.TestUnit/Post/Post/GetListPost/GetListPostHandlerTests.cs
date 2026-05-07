using System.Text.Json;
using be.Application.Common.Constants;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.Post.Post.GetListPost;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Post.GetListPost;

using PostAuthorQuery = PostAuthor;

public class GetListPostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly GetListPostHandler _handler;
    private readonly IPostReadRepository _postReadRepository;

    public GetListPostHandlerTests()
    {
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _format = Substitute.For<IFormat>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _encryption = Substitute.For<IEncryption>();

        _handler = new GetListPostHandler(
            _postReadRepository,
            _followReadRepository,
            _format,
            _currentUserContext,
            _encryption);
    }

    [Fact]
    public async Task Handle_WhenAnonymousRequestsLatest_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new GetListPostQuery(null, null, 10, TabPeriod.Latest), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập để xem tab này");
    }

    [Fact]
    public async Task Handle_WhenFollowingTabAndNoFollowees_ShouldReturnEmptyPage()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _followReadRepository.GetAllFolloweeIdPublicsAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());

        var result = await _handler.Handle(new GetListPostQuery(null, null, 10, TabPeriod.Following),
            CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Posts.Should().BeEmpty();
        result.Data.PageProfile.HasNextPage.Should().BeFalse();
        result.Data.PageProfile.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLatestWithCursor_ShouldDecryptAndUseDateCursor()
    {
        _currentUserContext.UserId.Returns("user-public-id");

        var cursor = new CursorPayload<DateTimeOffset>(new DateTimeOffset(2026, 4, 27, 12, 0, 0, TimeSpan.Zero), 5);
        var encryptedCursor = "encrypted-cursor";
        _encryption.Decrypt(encryptedCursor).Returns(JsonSerializer.Serialize(cursor));

        var postAuthor = new PostAuthorQuery { PublicUserId = "author-1", Name = "Alice", Avatar = Guid.NewGuid() };
        var items = new List<Post1Dto>
        {
            new()
            {
                IdPublic = Guid.NewGuid(),
                Content = "hello",
                TotalComment = 1,
                TotalDislike = 2,
                TotalLike = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                PostAuthor = postAuthor,
                PostImages = []
            }
        };

        _postReadRepository.GetListPostByDateAsync(null, null, 10,
                Arg.Is<CursorPayload<DateTimeOffset>?>(x => x != null && x.Id == 5), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>(items, false, null));
        _followReadRepository.GetFolloweeIdSetAsync("user-public-id", Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("avatar-url");

        var result = await _handler.Handle(new GetListPostQuery(null, encryptedCursor, 10, TabPeriod.Latest),
            CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Posts.Should().HaveCount(1);
        result.Data.PageProfile.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenScoreTab_ShouldUseScoreRepositoryAndMapImages()
    {
        _currentUserContext.UserId.Returns((string?)null);

        var postAuthor = new PostAuthorQuery { PublicUserId = "author-1", Name = "Alice", Avatar = Guid.NewGuid() };
        var imageId = Guid.NewGuid();
        var beforeId = Guid.NewGuid();
        var afterId = Guid.NewGuid();

        var items = new List<Post1Dto>
        {
            new()
            {
                IdPublic = Guid.NewGuid(),
                Content = "scored post",
                TotalComment = 4,
                TotalDislike = 1,
                TotalLike = 8,
                CreatedAt = DateTimeOffset.UtcNow,
                PostAuthor = postAuthor,
                PostImages =
                [
                    new PostSearchImageDto
                        { Type = ImageEnum.before, Image = imageId, Before = beforeId, After = afterId }
                ]
            }
        };

        _postReadRepository.GetListPostByScoreAsync(null, 10, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>(items, true, new CursorPayload<short>(10, 2)));
        _encryption.Encrypt(Arg.Any<string>()).Returns("next-cursor");
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("formatted-url");

        var result = await _handler.Handle(new GetListPostQuery(null, null, 10, null), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Posts.Should().HaveCount(1);
        result.Data.Posts[0].PostAuthor.UserId.Should().Be("author-1");
        result.Data.Posts[0].Images.Should().HaveCount(1);
        result.Data.PageProfile.HasNextPage.Should().BeTrue();
        result.Data.PageProfile.NextCursor.Should().Be("next-cursor");
    }

    [Fact]
    public async Task Handle_WhenLoggedIn_ShouldMarkFollowedAuthors()
    {
        _currentUserContext.UserId.Returns("user-public-id");

        var items = new List<Post1Dto>
        {
            new()
            {
                IdPublic = Guid.NewGuid(),
                Content = "hello",
                TotalComment = 0,
                TotalDislike = 0,
                TotalLike = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                PostAuthor = new PostAuthorQuery { PublicUserId = "author-1", Name = "Alice", Avatar = Guid.NewGuid() },
                PostImages = []
            }
        };

        _postReadRepository.GetListPostByDateAsync(null, null, 10, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>(items, false, null));
        _followReadRepository.GetFolloweeIdSetAsync("user-public-id", Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "author-1" });
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("avatar-url");

        var result = await _handler.Handle(new GetListPostQuery(null, null, 10, TabPeriod.Latest),
            CancellationToken.None);

        result.Data!.Posts[0].PostAuthor.IsFollow.Should().BeTrue();
    }
}