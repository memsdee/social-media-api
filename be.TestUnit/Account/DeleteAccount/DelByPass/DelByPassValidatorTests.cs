using be.Application.Features.Account.DeleteAccount.DelByPass;
using FluentValidation.TestHelper;

namespace Tests.Account.DeleteAccount.DelByPass;

public class DelByPassValidatorTests
{
    private readonly DelByPassValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Mail_Is_Empty()
    {
        var command = new DelByPassCommand { Mail = "", Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không được để trống");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password không được để trống");
    }

    [Fact]
    public void Should_Have_Error_When_Mail_Is_Invalid()
    {
        var command = new DelByPassCommand { Mail = "invalid_mail", Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không đúng định dạng");
    }

    [Fact]
    public void Should_Have_Error_When_Mail_Exceeds_Maximum_Length()
    {
        var command = new DelByPassCommand { Mail = new string('a', 246) + "@email.com", Password = "password123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Mail)
            .WithErrorMessage("Mail không được vượt quá 255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Length_Is_Less_Than_6()
    {
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "12345" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password phải có ít nhất 6 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Length_Exceeds_255()
    {
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password không được vượt quá 255 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new DelByPassCommand { Mail = "test@mail.com", Password = "valid_password" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}