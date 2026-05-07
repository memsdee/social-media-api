using System.Text;
using be.Application.Dtos.Queries.Account;
using be.Application.Features.Account.Change.ChangeEmail;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.Change.ChangeEmail;

public class ChangeEmailHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IDistributedCache _cache;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ChangeEmailHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeEmailHandlerTests()
    {
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _cache = Substitute.For<IDistributedCache>();

        _handler = new ChangeEmailHandler(
            _passwordHasher,
            _currentUserContext,
            _accountRepository,
            _unitOfWork,
            _outboxRepository,
            _cache);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new ChangeEmailCommand();

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new ChangeEmailCommand();

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "old@mail.com" });
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("OTP không đúng hoặc đã hết hạn");
    }

    [Fact]
    public async Task Handle_OtpWrongValue_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "old@mail.com" });
        var correctOtpInCache = Encoding.UTF8.GetBytes("654321");
        _cache.GetAsync("new@mail.com", Arg.Any<CancellationToken>()).Returns(correctOtpInCache);
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("OTP không đúng hoặc đã hết hạn");
    }

    [Fact]
    public async Task Handle_IsThirdPartyAccount_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "old@mail.com", Pass = null });
        var otpBytes = Encoding.UTF8.GetBytes("123456");
        _cache.GetAsync("new@mail.com", Arg.Any<CancellationToken>()).Returns(otpBytes);
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456", Pass = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản chỉ đăng nhập bằng bên thứ 3, không thể đổi email");
    }

    [Fact]
    public async Task Handle_SameEmail_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "same@mail.com", Pass = "hashed_pass" });
        var otpBytes = Encoding.UTF8.GetBytes("123456");
        _cache.GetAsync("same@mail.com", Arg.Any<CancellationToken>()).Returns(otpBytes);
        var command = new ChangeEmailCommand { NewEmail = "same@mail.com", Otp = "123456", Pass = "password" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Email mới không được trùng với email hiện tại");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "old@mail.com", Pass = "hashed_pass" });
        var otpBytes = Encoding.UTF8.GetBytes("123456");
        _cache.GetAsync("new@mail.com", Arg.Any<CancellationToken>()).Returns(otpBytes);
        _passwordHasher.Verify("wrong_pass", "hashed_pass").Returns(false);
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456", Pass = "wrong_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Mật khẩu không đúng");
    }

    [Fact]
    public async Task Handle_ValidRequest_Success()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Mail = "old@mail.com", Pass = "hashed_pass" });
        var otpBytes = Encoding.UTF8.GetBytes("123456");
        _cache.GetAsync("new@mail.com", Arg.Any<CancellationToken>()).Returns(otpBytes);
        _passwordHasher.Verify("correct_pass", "hashed_pass").Returns(true);
        var command = new ChangeEmailCommand { NewEmail = "new@mail.com", Otp = "123456", Pass = "correct_pass" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Đổi email thành công");
        await _accountRepository.Received(1).ChangeMailAsync(1, "new@mail.com", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}