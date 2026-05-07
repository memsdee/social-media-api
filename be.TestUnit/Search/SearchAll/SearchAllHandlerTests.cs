using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Search.SearchAll;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Search.SearchAll;

using PostAuthorQuery = PostAuthor;

public class SearchAllHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly SearchAllHandler _handler;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IUserReadRepository _userReadRepository;

    public SearchAllHandlerTests()
    {
        _userReadRepository = Substitute.For<IUserReadRepository>();
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _format = Substitute.For<IFormat>();
        _encryption = Substitute.For<IEncryption>();

        _handler = new SearchAllHandler(
            _userReadRepository,
            _postReadRepository,
            _followReadRepository,
            _currentUserContext,
            _format,
            _encryption);
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnResultsWithoutFollowLookup()
    {
        var query = new SearchAllQuery
        {
            Q = "  Hello  ",
            LimitUser = 10,
            LimitPost = 10
        };

        _currentUserContext.UserId.Returns((string?)null);

        var userCursor = new CursorPayload<short>(10, 1);
        var postCursor = new CursorPayload<short>(20, 2);
        var users = new List<UserSearchDto>
        {
            new()
            {
                PublicUserId = "user-1",
                Name = "Alice",
                Avatar = Guid.NewGuid(),
                TotalFollowers = 5
            }
        };

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
                PostAuthor = new PostAuthorQuery { PublicUserId = "author-1", Name = "Bob", Avatar = Guid.NewGuid() },
                AuthorReact = ReactEnum.like,
                MyReact = ReactEnum.dislike,
                PostImages =
                [
                    new PostSearchImageDto { Type = ImageEnum.after, Image = null, Before = null, After = null }
                ]
            }
        };

        _userReadRepository.SearchUsersByNameAsync("hello", query.LimitUser, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<UserSearchDto, CursorPayload<short>?>(users, true, userCursor));
        _postReadRepository.GetListPostByContentAsync("hello", query.LimitPost, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>(posts, false, postCursor));
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("img");
        _encryption.Encrypt(Arg.Any<string>()).Returns("post-cursor", "user-cursor");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        var data = result.Data!;
        data.User.Should().HaveCount(1);
        data.User[0].IsFollowing.Should().BeFalse();
        data.User[0].Avatar.Should().Be("img");
        data.Post.Should().HaveCount(1);
        data.Post[0].PostAuthor.IsFollow.Should().BeFalse();
        data.Post[0].MyReact.Should().BeNull();
        data.Post[0].Images.Should().NotBeNull();
        var images = data.Post[0].Images!;
        images[0].Image.Should().Be(string.Empty);
        images[0].Before.Should().BeNull();
        images[0].After.Should().BeNull();
        data.PageProfilePost.NextCursor.Should().Be("post-cursor");
        data.PageProfileUser.NextCursor.Should().Be("user-cursor");

        await _followReadRepository.DidNotReceive().GetFolloweeIdSetAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLoggedIn_ShouldSetFollowAndMyReact()
    {
        var query = new SearchAllQuery
        {
            Q = "hello",
            LimitUser = 10,
            LimitPost = 10
        };

        _currentUserContext.UserId.Returns("viewer");

        var users = new List<UserSearchDto>
        {
            new()
            {
                PublicUserId = "UserA",
                Name = "Alice",
                Avatar = Guid.NewGuid(),
                TotalFollowers = 2
            }
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
                PostAuthor = new PostAuthorQuery { PublicUserId = "usera", Name = "Bob", Avatar = Guid.NewGuid() },
                AuthorReact = ReactEnum.like,
                MyReact = ReactEnum.dislike,
                PostImages =
                [
                    new PostSearchImageDto
                    {
                        Type = ImageEnum.before,
                        Image = Guid.NewGuid(),
                        Before = Guid.NewGuid(),
                        After = Guid.NewGuid()
                    }
                ]
            }
        };

        _userReadRepository.SearchUsersByNameAsync(query.Q, query.LimitUser, null, Arg.Any<CancellationToken>())
            .Returns(new CursorResult<UserSearchDto, CursorPayload<short>?>(users, false, null));
        _postReadRepository.GetListPostByContentAsync(query.Q, query.LimitPost, null, Arg.Any<CancellationToken>())
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
        data.User.Should().HaveCount(1);
        data.User[0].IsFollowing.Should().BeTrue();
        data.Post.Should().HaveCount(1);
        data.Post[0].PostAuthor.IsFollow.Should().BeTrue();
        data.Post[0].IsReact.Should().Be(ReactEnum.like);
        data.Post[0].MyReact.Should().Be(ReactEnum.dislike);
        data.Post[0].Images.Should().NotBeNull();
        var images = data.Post[0].Images!;
        images[0].Image.Should().Be("img");
        images[0].Before.Should().Be("img");
        images[0].After.Should().Be("img");

        await _followReadRepository.Received(1).GetFolloweeIdSetAsync(
            "viewer",
            Arg.Is<IEnumerable<string>>(ids =>
                ids.Count() == 1 && ids.Contains("UserA", StringComparer.OrdinalIgnoreCase)),
            Arg.Any<CancellationToken>());
    }
}