using be.Application.Features.Account.Change.ChangeEmail;
using FluentValidation.TestHelper;

namespace Tests.Account.Change.ChangeEmail;

public class ChangeEmailValidatorTests
{
    private readonly ChangeEmailValidator _validator;

    public ChangeEmailValidatorTests()
    {
        _validator = new ChangeEmailValidator();
    }

    [Fact]
    public void Should_Have_Error_When_NewEmail_Is_Empty()
    {
        var command = new ChangeEmailCommand { NewEmail = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail)
            .WithErrorMessage("Mail không để trống");
    }

    [Fact]
    public void Should_Have_Error_When_NewEmail_Exceeds_Maximum_Length()
    {
        var command = new ChangeEmailCommand { NewEmail = new string('a', 246) + "@email.com" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail)
            .WithErrorMessage("Độ dài email tối đa 255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_NewEmail_Is_Invalid()
    {
        var command = new ChangeEmailCommand { NewEmail = "invalid_email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail)
            .WithErrorMessage("Định dạng mail không hợp lệ");
    }

    [Fact]
    public void Should_Have_Error_When_Otp_Is_Empty()
    {
        var command = new ChangeEmailCommand { NewEmail = "test@email.com", Otp = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Otp)
            .WithErrorMessage("OTP đang trống");
    }

    [Fact]
    public void Should_Have_Error_When_Pass_Is_Empty()
    {
        var command = new ChangeEmailCommand { NewEmail = "test@email.com", Otp = "123456", Pass = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Pass)
            .WithErrorMessage("Mật khẩu đang trống");
    }

    [Fact]
    public void Should_Have_Error_When_Pass_Length_Is_Less_Than_6()
    {
        var command = new ChangeEmailCommand { NewEmail = "test@email.com", Otp = "123456", Pass = "12345" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Pass)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_Pass_Length_Exceeds_255()
    {
        var command = new ChangeEmailCommand
            { NewEmail = "test@email.com", Otp = "123456", Pass = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Pass)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new ChangeEmailCommand { NewEmail = "test@email.com", Otp = "123456", Pass = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}