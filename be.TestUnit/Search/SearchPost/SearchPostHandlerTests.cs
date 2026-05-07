using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.Search.SearchPost;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Search.SearchPost;

using PostAuthorQuery = PostAuthor;

public class SearchPostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly SearchPostHandler _handler;
    private readonly IPostReadRepository _postReadRepository;

    public SearchPostHandlerTests()
    {
        _format = Substitute.For<IFormat>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _encryption = Substitute.For<IEncryption>();

        _handler = new SearchPostHandler(
            _format,
            _currentUserContext,
            _postReadRepository,
            _followReadRepository,
            _encryption);
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnResultsWithoutFollowLookup()
    {
        var query = new SearchPostQuery
        {
            Q = "  Hello  ",
            Limit = 10,
            Cursor = null
        };

        _currentUserContext.UserId.Returns((string?)null);

        var posts = new List<Post1Dto>
        {
            new()
            {
                IdPublic = Guid.NewGuid(),
                Content = "post",
                TotalComment = 1,
                TotalDislike = 0,
                TotalLike = 2,
                CreatedAt = DateTimeOffset.UtcNow,
                PostAuthor = new PostAuthorQuery { PublicUserId = "author-1", Name = "Alice", Avatar = Guid.NewGuid() },
                AuthorReact = ReactEnum.like,
                MyReact = ReactEnum.dislike,
                PostImages = new[]
                {
                    new PostSearchImageDto { Type = ImageEnum.after, Image = null, Before = null, After = null }
                }
            }
        };

        var nextCursor = new CursorPayload<short>(3, 7);

        _postReadRepository.GetListPostByContentAsync("hello", query.Limit, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>(posts, true, nextCursor));
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("img");
        _encryption.Encrypt(Arg.Any<string>()).Returns("next-cursor");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        var data = result.Data!;
        data.Posts.Should().HaveCount(1);
        data.Posts[0].MyReact.Should().BeNull();
        data.Posts[0].PostAuthor.IsFollow.Should().BeFalse();
        data.PageProfile.NextCursor.Should().Be("next-cursor");
        data.Posts[0].Images.Should().NotBeNull();

        var images = data.Posts[0].Images!;
        images[0].Image.Should().Be(string.Empty);
        images[0].Before.Should().BeNull();
        images[0].After.Should().BeNull();

        await _followReadRepository.DidNotReceive().GetFolloweeIdSetAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLoggedIn_ShouldUseCursorAndSetFollowAndMyReact()
    {
        var cursor = new CursorPayload<short>(5, 2);
        var encryptedCursor = "encrypted-cursor";

        _encryption.Decrypt(encryptedCursor).Returns(JsonSerializer.Serialize(cursor));
        _currentUserContext.UserId.Returns("viewer");

        var query = new SearchPostQuery
        {
            Q = "hello",
            Limit = 10,
            Cursor = encryptedCursor
        };

        var posts = new List<Post1Dto>
        {
            new()
            {
                IdPublic = Guid.NewGuid(),
                Content = "post",
                TotalComment = 0,
                TotalDislike = 1,
                TotalLike = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                PostAuthor = new PostAuthorQuery { PublicUserId = "UserA", Name = "Bob", Avatar = Guid.NewGuid() },
                AuthorReact = ReactEnum.like,
                MyReact = ReactEnum.dislike,
                PostImages = new[]
                {
                    new PostSearchImageDto
                    {
                        Type = ImageEnum.before,
                        Image = Guid.NewGuid(),
                        Before = Guid.NewGuid(),
                        After = Guid.NewGuid()
                    }
                }
            }
        };

        _postReadRepository.GetListPostByContentAsync(
                "hello",
                query.Limit,
                Arg.Is<CursorPayload<short>?>(value => value != null && value.Selector == 5 && value.Id == 2),
                Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>(posts, false, null));
        _followReadRepository.GetFolloweeIdSetAsync(
                "viewer",
                Arg.Is<IEnumerable<string>>(ids =>
                    ids.Count() == 1 && ids.Contains("UserA", StringComparer.OrdinalIgnoreCase)),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "usera" });
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("img");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        var data = result.Data!;
        data.Posts.Should().HaveCount(1);
        data.Posts[0].PostAuthor.IsFollow.Should().BeTrue();
        data.Posts[0].IsReact.Should().Be(ReactEnum.like);
        data.Posts[0].MyReact.Should().Be(ReactEnum.dislike);
        data.Posts[0].Images.Should().NotBeNull();

        var images = data.Posts[0].Images!;
        images[0].Image.Should().Be("img");
        images[0].Before.Should().Be("img");
        images[0].After.Should().Be("img");
    }
}