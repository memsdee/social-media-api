using be.Application.Features.Account.ForgetPass;
using FluentValidation.TestHelper;

namespace Tests.Account.ForgetPass;

public class ForgetPassValidatorTests
{
    private readonly ForgetPassValidator _validator;

    public ForgetPassValidatorTests()
    {
        _validator = new ForgetPassValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Mail_Is_Empty()
    {
        var command = new ForgetPassCommand { Mail = "", Otp = "123456", NewPassword = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không được trống");
    }

    [Fact]
    public void Should_Have_Error_When_Mail_Is_Invalid()
    {
        var command = new ForgetPassCommand { Mail = "invalid_mail", Otp = "123456", NewPassword = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không đúng định dạng");
    }

    [Fact]
    public void Should_Have_Error_When_Mail_Exceeds_Maximum_Length()
    {
        var command = new ForgetPassCommand
            { Mail = new string('a', 246) + "@email.com", Otp = "123456", NewPassword = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không được vượt quá 255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_Otp_Is_Empty()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "", NewPassword = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Otp)
            .WithErrorMessage("Otp không được trống");
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Empty()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Mật khẩu mới không được trống");
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Length_Is_Less_Than_6()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "12345" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Length_Exceeds_255()
    {
        var command = new ForgetPassCommand
            { Mail = "test@mail.com", Otp = "123456", NewPassword = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new ForgetPassCommand { Mail = "test@mail.com", Otp = "123456", NewPassword = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}