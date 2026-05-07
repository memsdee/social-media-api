using be.Application.Features.Account.Change.EditAccount;
using FluentValidation.TestHelper;

namespace Tests.Account.Change.EditAccount;

public class EditAccountValidatorTests
{
    private readonly EditAccountCommandValidator _validator;

    public EditAccountValidatorTests()
    {
        _validator = new EditAccountCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_All_Fields_Are_Null()
    {
        var command = new EditAccountCommand();
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Vui lòng cung cấp ít nhất một trường để cập nhật");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Length_Is_Less_Than_3()
    {
        var command = new EditAccountCommand { UserName = "ab" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("Độ dài UserName từ 3-50 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Length_Exceeds_50()
    {
        var command = new EditAccountCommand { UserName = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("Độ dài UserName từ 3-50 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Has_Multiple_Consecutive_Spaces()
    {
        var command = new EditAccountCommand { UserName = "abc  def" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("Không được có khoảng trắng đầu/cuối hoặc nhiều khoảng trắng liên tiếp");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Length_Is_Less_Than_3()
    {
        var command = new EditAccountCommand { UserId = "ab" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Độ dài UserId từ 3-50 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Length_Exceeds_50()
    {
        var command = new EditAccountCommand { UserId = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Độ dài UserId từ 3-50 ký tự");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Contains_Invalid_Characters()
    {
        var command = new EditAccountCommand { UserId = "invalid-id!" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Userid chỉ được chứa chữ cái, số và dấu gạch dưới");
    }

    [Fact]
    public void Should_Have_Error_When_Bio_Length_Exceeds_160()
    {
        var command = new EditAccountCommand { Bio = new string('a', 161) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Bio)
            .WithErrorMessage("Độ dài Bio tối đa 160 ký tự");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        var command = new EditAccountCommand { UserName = "valid name", UserId = "valid_id_123", Bio = "Valid bio" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}