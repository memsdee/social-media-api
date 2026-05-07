using be.Application.Common.Constants;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.Post.Post.GetListPost;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using NSubstitute;

namespace be.Tests.Unit.Features.Post;

public class GetListPostHandlerTests
{
    private static readonly string CurrentUserId = "user-001";
    private readonly ICurrentUserContext _currentUserContext = Substitute.For<ICurrentUserContext>();
    private readonly IEncryption _encryption = Substitute.For<IEncryption>();
    private readonly IFollowReadRepository _followReadRepository = Substitute.For<IFollowReadRepository>();
    private readonly IFormat _format = Substitute.For<IFormat>();

    private readonly GetListPostHandler _handler;
    private readonly IPostReadRepository _postReadRepository = Substitute.For<IPostReadRepository>();

    public GetListPostHandlerTests()
    {
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>())
            .Returns("https://cdn.example.com/image.webp");

        _handler = new GetListPostHandler(
            _postReadRepository,
            _followReadRepository,
            _format,
            _currentUserContext,
            _encryption);
    }

    private static Post1Dto MakePost(string authorId = "author-001")
    {
        return new Post1Dto
        {
            IdPublic = Guid.NewGuid(),
            Content = "Test content",
            TotalComment = 0,
            TotalLike = 5,
            TotalDislike = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            Sequence = 1,
            Score = 10,
            PostAuthor = new PostAuthor
            {
                PublicUserId = authorId,
                Name = "Author Name",
                Avatar = Guid.NewGuid()
            },
            PostImages = []
        };
    }

    [Fact]
    public async Task Handle_TrendingTab_QueriesByScore()
    {
        _currentUserContext.UserId.Returns((string?)null);

        _postReadRepository
            .GetListPostByScoreAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CursorPayload<short>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>([MakePost()], false, null));

        _followReadRepository
            .GetFolloweeIdSetAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetListPostQuery(null, null, 10, "trending");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Posts);
        await _postReadRepository.Received(1)
            .GetListPostByScoreAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CursorPayload<short>?>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LatestTab_QueriesByDate()
    {
        _currentUserContext.UserId.Returns(CurrentUserId);

        _postReadRepository
            .GetListPostByDateAsync(Arg.Any<string?>(), Arg.Any<HashSet<string>?>(), Arg.Any<int>(),
                Arg.Any<CursorPayload<DateTimeOffset>?>(), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>([MakePost()], false, null));

        _followReadRepository
            .GetFolloweeIdSetAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetListPostQuery(null, null, 10, TabPeriod.Latest);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Posts);
        await _postReadRepository.Received(1)
            .GetListPostByDateAsync(Arg.Any<string?>(), Arg.Any<HashSet<string>?>(), Arg.Any<int>(),
                Arg.Any<CursorPayload<DateTimeOffset>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FollowingTab_ReturnsEmptyWhenNoFollowees()
    {
        _currentUserContext.UserId.Returns(CurrentUserId);
        _followReadRepository
            .GetAllFolloweeIdPublicsAsync(CurrentUserId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetListPostQuery(null, null, 10, TabPeriod.Following);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Posts);
        Assert.False(result.Data.PageProfile.HasNextPage);
    }

    [Fact]
    public async Task Handle_FollowingTab_QueriesByDateWithFolloweeIds()
    {
        _currentUserContext.UserId.Returns(CurrentUserId);

        var followeeIds = new HashSet<string> { "author-001" };
        _followReadRepository
            .GetAllFolloweeIdPublicsAsync(CurrentUserId, Arg.Any<CancellationToken>())
            .Returns(followeeIds);

        _postReadRepository
            .GetListPostByDateAsync(Arg.Any<string?>(),
                Arg.Is<HashSet<string>?>(s => s != null && s.Contains("author-001")), Arg.Any<int>(),
                Arg.Any<CursorPayload<DateTimeOffset>?>(), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>([MakePost()], false, null));

        _followReadRepository
            .GetFolloweeIdSetAsync(CurrentUserId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(followeeIds);

        var query = new GetListPostQuery(null, null, 10, TabPeriod.Following);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Posts);
    }

    [Fact]
    public async Task Handle_FollowingTabNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        var query = new GetListPostQuery(null, null, 10, TabPeriod.Following);

        await Assert.ThrowsAsync<CustomException.UnauthorizedException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_LatestTabNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        var query = new GetListPostQuery(null, null, 10, TabPeriod.Latest);

        await Assert.ThrowsAsync<CustomException.UnauthorizedException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_LoggedIn_SetsIsFollowCorrectly()
    {
        _currentUserContext.UserId.Returns(CurrentUserId);

        var followedAuthorId = "author-followed";
        var unfollowedAuthorId = "author-unfollowed";

        _postReadRepository
            .GetListPostByScoreAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CursorPayload<short>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>(
                [MakePost(followedAuthorId), MakePost(unfollowedAuthorId)], false, null));

        _followReadRepository
            .GetFolloweeIdSetAsync(CurrentUserId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { followedAuthorId });

        var query = new GetListPostQuery(null, null, 10, "trending");

        var result = await _handler.Handle(query, CancellationToken.None);

        var followed = result.Data!.Posts.First(p => p.PostAuthor.UserId == followedAuthorId);
        var unfollowed = result.Data!.Posts.First(p => p.PostAuthor.UserId == unfollowedAuthorId);

        Assert.True(followed.PostAuthor.IsFollow);
        Assert.False(unfollowed.PostAuthor.IsFollow);
    }

    [Fact]
    public async Task Handle_HasNextPage_ReturnsEncryptedCursor()
    {
        _currentUserContext.UserId.Returns((string?)null);

        _postReadRepository
            .GetListPostByScoreAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CursorPayload<short>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new CursorResult<Post1Dto, CursorPayload<short>?>([MakePost()], true,
                new CursorPayload<short>(8, 42)));

        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted-cursor");

        var query = new GetListPostQuery(null, null, 10, "trending");

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.Data!.PageProfile.HasNextPage);
        Assert.Equal("encrypted-cursor", result.Data.PageProfile.NextCursor);
    }
}