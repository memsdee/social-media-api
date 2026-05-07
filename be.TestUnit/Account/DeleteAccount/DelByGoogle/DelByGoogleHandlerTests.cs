using be.Application.Dtos.EventBus;
using be.Application.Dtos.OAuth;
using be.Application.Dtos.Queries.Account;
using be.Application.Features.Account.DeleteAccount.DelByGoogle;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.DeleteAccount.DelByGoogle;

public class DelByGoogleHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGoogle _google;
    private readonly DelByGoogleHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IThirdPartyLoginsRepository _thirdPartyLoginsRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;

    public DelByGoogleHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _thirdPartyLoginsRepository = Substitute.For<IThirdPartyLoginsRepository>();
        _google = Substitute.For<IGoogle>();
        _tokenRepository = Substitute.For<ITokenRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new DelByGoogleHandler(
            _currentUserContext,
            _unitOfWork,
            _accountRepository,
            _thirdPartyLoginsRepository,
            _google,
            _tokenRepository,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserAlreadyDeleted_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { IsDeleted = true });
        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotThirdParty_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { IsDeleted = false, IsThirdParty = false });
        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản này không liên kết Google. Vui lòng xóa bằng mật khẩu");
    }

    [Fact]
    public async Task Handle_GoogleVerificationFails_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { PrivateAccountId = 1, IsDeleted = false, IsThirdParty = true });

        _google.GetTokenAsync("code", Arg.Any<CancellationToken>()).Returns(new GoogleDto { IdToken = "id_token" });
        _google.VerifyIdTokenAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfoDto { Subject = "google_subject" });

        _thirdPartyLoginsRepository
            .AnyThirdPartyAsync(1, ThirdPartyLoginEnum.google, "google_subject", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Xác thực Google không hợp lệ");
    }

    [Fact]
    public async Task Handle_ValidRequest_Success()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { PrivateAccountId = 1, IsDeleted = false, IsThirdParty = true });

        _google.GetTokenAsync("code", Arg.Any<CancellationToken>()).Returns(new GoogleDto { IdToken = "id_token" });
        _google.VerifyIdTokenAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfoDto { Subject = "google_subject" });

        _thirdPartyLoginsRepository
            .AnyThirdPartyAsync(1, ThirdPartyLoginEnum.google, "google_subject", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DelByGoogleCommand { GoogleCode = "code" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _accountRepository.Received(1)
            .UpdateStatusAsync(1, StatusAccountEnum.deleted, Arg.Any<CancellationToken>());
        await _tokenRepository.Received(1).DelByPrivateAccountIdAsync(1, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.delAccount,
            Arg.Is<DelAccountEvent>(e => e.PrivateAccountId == 1), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TransactionRollback_WhenExceptionOccursDuringUpdate()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { PrivateAccountId = 1, IsDeleted = false, IsThirdParty = true });

        _google.GetTokenAsync("code", Arg.Any<CancellationToken>()).Returns(new GoogleDto { IdToken = "id_token" });
        _google.VerifyIdTokenAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfoDto { Subject = "google_subject" });

        _thirdPartyLoginsRepository
            .AnyThirdPartyAsync(1, ThirdPartyLoginEnum.google, "google_subject", Arg.Any<CancellationToken>())
            .Returns(true);

        _accountRepository.UpdateStatusAsync(1, StatusAccountEnum.deleted, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Database error")));

        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GoogleGetTokenAsyncThrows_ThrowsException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { PrivateAccountId = 1, IsDeleted = false, IsThirdParty = true });

        _google.GetTokenAsync("code", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GoogleDto>(new Exception("Google API Error")));

        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Google API Error");
    }

    [Fact]
    public async Task Handle_GoogleVerifyIdTokenAsyncThrows_ThrowsException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount9Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account9Dto { PrivateAccountId = 1, IsDeleted = false, IsThirdParty = true });

        _google.GetTokenAsync("code", Arg.Any<CancellationToken>()).Returns(new GoogleDto { IdToken = "id_token" });
        _google.VerifyIdTokenAsync("id_token", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GoogleUserInfoDto>(new Exception("Google Verify Error")));

        var command = new DelByGoogleCommand { GoogleCode = "code" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Google Verify Error");
    }
}