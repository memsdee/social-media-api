using be.Application.Dtos.Queries.Account;
using be.Application.Dtos.Queries.ThirdPartyIdentity;
using be.Application.Features.Account.Link.UnlinkGoogle;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.Link.UnlinkGoogle;

public class UnlinkGoogleHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly UnlinkGoogleHandler _handler;
    private readonly IThirdPartyLoginsRepository _thirdPartyLoginsRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;

    public UnlinkGoogleHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _thirdPartyLoginsRepository = Substitute.For<IThirdPartyLoginsRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new UnlinkGoogleHandler(
            _currentUserContext,
            _accountRepository,
            _thirdPartyLoginsRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        var command = new UnlinkGoogleCommand();
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldThrowUnauthorizedException()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).ReturnsNull();

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenNotLinkedToGoogle_ShouldThrowBusinessValidationException()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1 };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản chưa liên kết Google");
    }

    [Fact]
    public async Task Handle_WhenNoGoogleInThirdPartyState_ShouldThrowBusinessValidationException()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1 };
        var thirdPartyState = new ThirdParty2Dto { HasGoogle = false };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Returns(thirdPartyState);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản chưa liên kết Google");
    }

    [Fact]
    public async Task Handle_WhenGoogleIsOnlyLoginMethod_ShouldThrowBusinessValidationException()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1, HasPass = false };
        var thirdPartyState = new ThirdParty2Dto { HasGoogle = true, HasOtherThirdParty = false };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Returns(thirdPartyState);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản chỉ liên kết Google và không có mật khẩu, không thể gỡ liên kết");
    }

    [Fact]
    public async Task Handle_WhenSuccessWithPass_ShouldCommitTransactionAndReturnTrue()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1, HasPass = true };
        var thirdPartyState = new ThirdParty2Dto { HasGoogle = true, HasOtherThirdParty = false };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Returns(thirdPartyState);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeTrue();
        result.Message.Should().Be("Gỡ liên kết Google thành công");
        await _thirdPartyLoginsRepository.Received(1)
            .DelAsync(accountDto.PrivateAccountId, Arg.Any<CancellationToken>());
        await _accountRepository.Received(1)
            .UpdateThirdPartyAsync(accountDto.PrivateAccountId, false, Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSuccessWithOtherThirdParty_ShouldCommitTransactionAndReturnTrue()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1, HasPass = false };
        var thirdPartyState = new ThirdParty2Dto { HasGoogle = true, HasOtherThirdParty = true };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Returns(thirdPartyState);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeTrue();
        result.Message.Should().Be("Gỡ liên kết Google thành công");
        await _thirdPartyLoginsRepository.Received(1)
            .DelAsync(accountDto.PrivateAccountId, Arg.Any<CancellationToken>());
        await _accountRepository.Received(1)
            .UpdateThirdPartyAsync(accountDto.PrivateAccountId, true, Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        var command = new UnlinkGoogleCommand();
        var userId = "userId";
        var accountDto = new Account7Dto { PrivateAccountId = 1, HasPass = true };
        var thirdPartyState = new ThirdParty2Dto { HasGoogle = true, HasOtherThirdParty = false };

        _currentUserContext.UserId.Returns(userId);
        _accountRepository.GetAccount7Async(userId, Arg.Any<CancellationToken>()).Returns(accountDto);
        _thirdPartyLoginsRepository.GetThirdParty2Async(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Returns(thirdPartyState);
        _thirdPartyLoginsRepository.DelAsync(accountDto.PrivateAccountId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}