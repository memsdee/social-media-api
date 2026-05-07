using be.Application.Dtos.Queries.Account;
using be.Application.Features.Account.Change.ChangePassword;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.Change.ChangePassword;

public class ChangePasswordHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ChangePasswordHandler _handler;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandlerTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var outboxRepository = Substitute.For<IOutboxRepository>();

        _handler = new ChangePasswordHandler(
            _accountRepository,
            _currentUserContext,
            _passwordHasher,
            _unitOfWork,
            outboxRepository);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new ChangePasswordCommand { OldPass = "old_pass", NewPass = "new_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_SamePassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        var command = new ChangePasswordCommand { OldPass = "same_pass", NewPass = "same_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Mật khẩu mới không được giống mật khẩu cũ");
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new ChangePasswordCommand { OldPass = "old_pass", NewPass = "new_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_AccountWithoutPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = null });
        var command = new ChangePasswordCommand { OldPass = "old_pass", NewPass = "new_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản chưa thiết lập mật khẩu, không thể đổi mật khẩu");
    }

    [Fact]
    public async Task Handle_WrongOldPassword_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = "hashed_pass" });
        _passwordHasher.Verify("wrong_old_pass", "hashed_pass").Returns(false);
        var command = new ChangePasswordCommand { OldPass = "wrong_old_pass", NewPass = "new_pass" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Mật khẩu cũ không đúng");
    }

    [Fact]
    public async Task Handle_ValidRequest_Success()
    {
        _currentUserContext.UserId.Returns("user-id");
        _accountRepository.GetAccount1Async("user-id", Arg.Any<CancellationToken>())
            .Returns(new Account1Dto { AccountId = 1, Pass = "hashed_pass" });
        _passwordHasher.Verify("correct_old_pass", "hashed_pass").Returns(true);
        _passwordHasher.Hash("new_pass").Returns("new_hashed_pass");
        var command = new ChangePasswordCommand { OldPass = "correct_old_pass", NewPass = "new_pass" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Đổi mật khẩu thành công");
        await _accountRepository.Received(1).ChangePasswordAsync(1, "new_hashed_pass", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}