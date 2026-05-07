using be.Application.Features.User.Unfollow;
using FluentValidation.TestHelper;

namespace Tests.User.Unfollow;

public class UnfollowValidatorTests
{
    private readonly UnfollowValidator _validator = new();

    [Fact]
    public void Validator_WhenUseridEmpty_ShouldHaveError()
    {
        var command = new UnfollowCommand(string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Userid)
            .WithErrorMessage("Userid không được trống");
    }

    [Fact]
    public void Validator_WhenUseridTooShort_ShouldHaveError()
    {
        var command = new UnfollowCommand("ab");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Userid)
            .WithErrorMessage("Độ dài userid từ 3-50 ký tự");
    }

    [Fact]
    public void Validator_WhenUseridTooLong_ShouldHaveError()
    {
        var command = new UnfollowCommand(new string('a', 51));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Userid)
            .WithErrorMessage("Độ dài userid từ 3-50 ký tự");
    }

    [Fact]
    public void Validator_WhenUseridHasInvalidChars_ShouldHaveError()
    {
        var command = new UnfollowCommand("user-1");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Userid)
            .WithErrorMessage("Userid chỉ được chứa chữ cái, số và dấu gạch dưới");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new UnfollowCommand("user_123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}