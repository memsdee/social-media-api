using be.Application.Features.Account.CheckPass;
using FluentValidation.TestHelper;

namespace Tests.Account.CheckPass;

public class CheckPassValidatorTests
{
    private readonly CheckPassValidator _validator;

    public CheckPassValidatorTests()
    {
        _validator = new CheckPassValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new CheckPassCommand { Password = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password không được để trống.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Length_Is_Less_Than_6()
    {
        var command = new CheckPassCommand { Password = "12345" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Length_Exceeds_255()
    {
        var command = new CheckPassCommand { Password = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new CheckPassCommand { Password = "valid_password" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}