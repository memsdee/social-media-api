using System.Text;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.ForgetPass;
using be.Application.Interfaces.Databases.Write;
using be.Domain;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Tests.Account.ForgetPass;

public class ForgetPassHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IDistributedCache _cache;
    private readonly ForgetPassHandler _handler;
    private readonly IUnitOfWork _unitOfWork;

    public ForgetPassHandlerTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _cache = Substitute.For<IDistributedCache>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ForgetPassHandler(_accountRepository, _cache, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidOtpAndMail_ShouldChangePasswordAndReturnBaseResponse()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "newPassword" };
        var otpBytes = Encoding.UTF8.GetBytes("123456");

        _cache.GetAsync(command.Mail, Arg.Any<CancellationToken>()).Returns(otpBytes);
        _accountRepository.ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeOfType<BaseResponse>();
        await _accountRepository.Received(1)
            .ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullOtpInCache_ShouldThrowBusinessValidationException()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "newPassword" };

        _cache.GetAsync(command.Mail, Arg.Any<CancellationToken>()).Returns((byte[])null!);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("OTP không đúng hoặc đã hết hạn");
        await _accountRepository.DidNotReceive()
            .ChangePasswordByMailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidOtp_ShouldThrowBusinessValidationException()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "newPassword" };
        var otpBytes = Encoding.UTF8.GetBytes("654321");

        _cache.GetAsync(command.Mail, Arg.Any<CancellationToken>()).Returns(otpBytes);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("OTP không đúng hoặc đã hết hạn");
        await _accountRepository.DidNotReceive()
            .ChangePasswordByMailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldThrowBusinessValidationException()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "newPassword" };
        var otpBytes = Encoding.UTF8.GetBytes("123456");

        _cache.GetAsync(command.Mail, Arg.Any<CancellationToken>()).Returns(otpBytes);
        _accountRepository.ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(0);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Không tìm thấy tài khoản với email này");
        await _accountRepository.Received(1)
            .ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMultipleAccountsFound_ShouldThrowBusinessValidationException()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "newPassword" };
        var otpBytes = Encoding.UTF8.GetBytes("123456");

        _cache.GetAsync(command.Mail, Arg.Any<CancellationToken>()).Returns(otpBytes);
        _accountRepository.ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(2);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Có lỗi xảy ra, vui lòng đăng nhập lại");
        await _accountRepository.Received(1)
            .ChangePasswordByMailAsync(command.Mail, command.NewPassword, Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}