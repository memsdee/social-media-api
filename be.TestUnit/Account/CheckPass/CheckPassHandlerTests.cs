using be.Application.Dtos.Queries.Account;
using be.Application.Features.Account.CheckPass;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.CheckPass;

public class CheckPassHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly CheckPassHandler _handler;
    private readonly IPasswordHasher _passwordHasher;

    public CheckPassHandlerTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _passwordHasher = Substitute.For<IPasswordHasher>();

        _handler = new CheckPassHandler(
            _accountRepository,
            _currentUserContext,
            _passwordHasher);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new CheckPassCommand { Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new CheckPassCommand { Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_PasswordIsNullOrEmpty_ReturnsFalse()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = null });
        var command = new CheckPassCommand { Password = "password" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeFalse();
        result.Message.Should().Be("Mật khẩu không đúng");
    }

    [Fact]
    public async Task Handle_PasswordIsWrong_ReturnsFalse()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = "hashed_pass" });
        _passwordHasher.Verify("wrong_pass", "hashed_pass").Returns(false);
        var command = new CheckPassCommand { Password = "wrong_pass" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeFalse();
        result.Message.Should().Be("Mật khẩu không đúng");
    }

    [Fact]
    public async Task Handle_ValidPassword_ReturnsTrue()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = "hashed_pass" });
        _passwordHasher.Verify("correct_pass", "hashed_pass").Returns(true);
        var command = new CheckPassCommand { Password = "correct_pass" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeTrue();
        result.Message.Should().Be("Mật khẩu đúng");
    }
}