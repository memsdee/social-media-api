using be.Application.Features.Account.Change.ChangePassword;
using FluentValidation.TestHelper;

namespace Tests.Account.Change.ChangePassword;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_OldPass_Is_Empty()
    {
        var command = new ChangePasswordCommand { OldPass = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OldPass)
            .WithErrorMessage("Mật khẩu cũ đang trống");
    }

    [Fact]
    public void Should_Have_Error_When_NewPass_Is_Empty()
    {
        var command = new ChangePasswordCommand { OldPass = "old_password", NewPass = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPass)
            .WithErrorMessage("Mật khẩu mới đang trống");
    }

    [Fact]
    public void Should_Have_Error_When_NewPass_Length_Is_Less_Than_6()
    {
        var command = new ChangePasswordCommand { OldPass = "old_password", NewPass = "12345" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPass)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_NewPass_Length_Exceeds_255()
    {
        var command = new ChangePasswordCommand { OldPass = "old_password", NewPass = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPass)
            .WithErrorMessage("Mật khẩu dài từ 6-255 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new ChangePasswordCommand { OldPass = "old_password", NewPass = "new_password" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}