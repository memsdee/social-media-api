using be.Application.Dtos.Queries.Account;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.GetLoginMethod;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.GetLoginMethod;

public class GetLoginMethodHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly GetLoginMethodHandler _handler;

    public GetLoginMethodHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _accountRepository = Substitute.For<IAccountRepository>();

        _handler = new GetLoginMethodHandler(_currentUserContext, _accountRepository);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowCustomUnauthorizedException()
    {
        var query = new GetLoginMethodQuery();
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldThrowCustomUnauthorizedException()
    {
        var query = new GetLoginMethodQuery();
        var currentUserId = "userId123";
        _currentUserContext.UserId.Returns(currentUserId);
        _accountRepository.GetAccount5Async(currentUserId, Arg.Any<CancellationToken>()).ReturnsNull();

        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại!");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnGetLoginMethodResponse()
    {
        var query = new GetLoginMethodQuery();
        var currentUserId = "userId123";
        var account5Dto = new Account5Dto
        {
            HasMail = true,
            HasGoogle = false
        };

        _currentUserContext.UserId.Returns(currentUserId);
        _accountRepository.GetAccount5Async(currentUserId, Arg.Any<CancellationToken>()).Returns(account5Dto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.HasMail.Should().BeTrue();
        result.Data.HasGoogle.Should().BeFalse();
        result.Should().BeOfType<BaseResponse<GetLoginMethodResponse>>();
    }
}