using be.Application.Common.Constants;
using be.Application.Features.Post.Post.GetListPost;
using FluentValidation.TestHelper;

namespace Tests.Post.Post.GetListPost;

public class GetListPostValidatorTests
{
    private readonly GetListPostValidator _validator = new();

    [Fact]
    public void Validator_WhenTargetIdTooShort_ShouldHaveError()
    {
        var query = new GetListPostQuery("ab", null, 10, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("Độ dài userid từ 3-50 ký tự");
    }

    [Fact]
    public void Validator_WhenTargetIdHasInvalidCharacters_ShouldHaveError()
    {
        var query = new GetListPostQuery("abc-123", null, 10, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("UserId chỉ được chứa chữ cái, số và dấu gạch dưới");
    }

    [Fact]
    public void Validator_WhenLimitInvalid_ShouldHaveError()
    {
        var query = new GetListPostQuery(null, null, 0, null);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit không được trống");
    }

    [Fact]
    public void Validator_WhenTargetIdProvidedWithTab_ShouldHaveError()
    {
        var query = new GetListPostQuery("abc123", null, 10, TabPeriod.Following);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Tab)
            .WithErrorMessage("Khi có targetId thì tab phải là rỗng");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetListPostQuery(null, null, 10, TabPeriod.Latest);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}