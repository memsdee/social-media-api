using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Search.SearchAccount;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using FluentAssertions;
using NSubstitute;

namespace Tests.Search.SearchAccount;

public class SearchAccountHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEncryption _encryption;
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFormat _format;
    private readonly SearchAccountHandler _handler;
    private readonly IUserReadRepository _userReadRepository;

    public SearchAccountHandlerTests()
    {
        _userReadRepository = Substitute.For<IUserReadRepository>();
        _followReadRepository = Substitute.For<IFollowReadRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _encryption = Substitute.For<IEncryption>();
        _format = Substitute.For<IFormat>();

        _handler = new SearchAccountHandler(
            _userReadRepository,
            _followReadRepository,
            _currentUserContext,
            _encryption,
            _format);
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnUsersWithIsFollowingFalse()
    {
        var query = new SearchAccountQuery { Q = "test", Limit = 10 };
        var users = new List<UserSearchDto>
        {
            new() { PublicUserId = "user-1", Name = "User One", Avatar = Guid.NewGuid(), TotalFollowers = 5 }
        };
        var cursorResult = new CursorResult<UserSearchDto, CursorPayload<short>?>(users, false, null);

        _currentUserContext.UserId.Returns((string?)null);
        _userReadRepository.SearchUsersByNameAsync("test", 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("avatar-url");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.User.Should().HaveCount(1);
        result.Data.User[0].IsFollowing.Should().BeFalse();
        await _followReadRepository.DidNotReceiveWithAnyArgs().GetFolloweeIdSetAsync(Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLoggedIn_ShouldMarkFollowedUsers()
    {
        var query = new SearchAccountQuery { Q = "test", Limit = 10 };
        var users = new List<UserSearchDto>
        {
            new() { PublicUserId = "user-1", Name = "User One", Avatar = Guid.NewGuid(), TotalFollowers = 5 },
            new() { PublicUserId = "user-2", Name = "User Two", Avatar = Guid.NewGuid(), TotalFollowers = 10 }
        };
        var cursorResult = new CursorResult<UserSearchDto, CursorPayload<short>?>(users, false, null);

        _currentUserContext.UserId.Returns("current-user-id");
        _userReadRepository.SearchUsersByNameAsync("test", 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _followReadRepository.GetFolloweeIdSetAsync("current-user-id", Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "user-1" });
        _format.FormatImageUrl(Arg.Any<Guid?>(), Arg.Any<string>()).Returns("avatar-url");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data!.User.Should().HaveCount(2);
        result.Data.User.First(x => x.UserId == "user-1").IsFollowing.Should().BeTrue();
        result.Data.User.First(x => x.UserId == "user-2").IsFollowing.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldDecryptAndUseCursor()
    {
        var encryptedCursor = "encrypted-cursor";
        var query = new SearchAccountQuery { Q = "test", Limit = 10, Cursor = encryptedCursor };
        var cursor = new CursorPayload<short>(100, 5);

        _encryption.Decrypt(encryptedCursor).Returns(JsonSerializer.Serialize(cursor));
        _userReadRepository.SearchUsersByNameAsync("test", 10,
                Arg.Is<CursorPayload<short>?>(c => c != null && c.Selector == 100), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<UserSearchDto, CursorPayload<short>?>(new List<UserSearchDto>(), false, null));

        await _handler.Handle(query, CancellationToken.None);

        await _userReadRepository.Received(1)
            .SearchUsersByNameAsync("test", 10, Arg.Any<CursorPayload<short>?>(), Arg.Any<CancellationToken>());
    }
}