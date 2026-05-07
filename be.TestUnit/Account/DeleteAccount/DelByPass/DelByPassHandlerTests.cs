using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.Account;
using be.Application.Features.Account.DeleteAccount.DelByPass;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.DeleteAccount.DelByPass;

public class DelByPassHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly DelByPassHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenRepository _tokenRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;

    public DelByPassHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tokenRepository = Substitute.For<ITokenRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new DelByPassHandler(
            _currentUserContext,
            _accountRepository,
            _passwordHasher,
            _unitOfWork,
            _tokenRepository,
            _outboxRepository);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserAlreadyDeleted_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto { IsDeleted = true });
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_MissingMailOrPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto { IsDeleted = false, Mail = null, Pass = "hashed_pass" });
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản này không hỗ trợ xóa bằng mật khẩu. Vui lòng xóa bằng cách khác");
    }

    [Fact]
    public async Task Handle_MissingPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto { IsDeleted = false, Mail = "test@mail.com", Pass = null });
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản này không hỗ trợ xóa bằng mật khẩu. Vui lòng xóa bằng cách khác");
    }

    [Fact]
    public async Task Handle_WrongEmail_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto { IsDeleted = false, Mail = "correct@mail.com", Pass = "hashed_pass" });
        var command = new DelByPassCommand { Mail = "wrong@mail.com", Password = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Email không đúng");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto { IsDeleted = false, Mail = "test@mail.com", Pass = "hashed_pass" });
        _passwordHasher.Verify("wrong_password", "hashed_pass").Returns(false);
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "wrong_password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Mật khẩu không đúng");
    }

    [Fact]
    public async Task Handle_ValidRequest_Success()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto
                { PrivateAccountId = 1, IsDeleted = false, Mail = "test@mail.com", Pass = "hashed_pass" });
        _passwordHasher.Verify("correct_password", "hashed_pass").Returns(true);
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "correct_password" };

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
        _accountRepository.GetAccount8Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account8Dto
                { PrivateAccountId = 1, IsDeleted = false, Mail = "test@mail.com", Pass = "hashed_pass" });
        _passwordHasher.Verify("correct_password", "hashed_pass").Returns(true);

        _accountRepository.UpdateStatusAsync(1, StatusAccountEnum.deleted, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Database error")));

        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "correct_password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}