using be.Application.Features.User.GetListImage;
using FluentValidation.TestHelper;

namespace Tests.User.GetListImage;

public class GetListImageValidatorTests
{
    private readonly GetListImageValidator _validator = new();

    [Fact]
    public void Validator_WhenTargetIdEmpty_ShouldHaveError()
    {
        var query = new GetListImageQuery("", null, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("UserId không được để trống");
    }

    [Fact]
    public void Validator_WhenTargetIdInvalidFormat_ShouldHaveError()
    {
        var query = new GetListImageQuery("user-with-dash", null, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("UserId chỉ được chứa chữ cái, số và dấu gạch dưới");
    }

    [Fact]
    public void Validator_WhenLimitZero_ShouldHaveError()
    {
        var query = new GetListImageQuery("valid_user", null, 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit từ 1-50");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetListImageQuery("valid_user", null, 10);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}