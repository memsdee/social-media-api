using be.Application.Features.Notification.GetListNoti;
using FluentValidation.TestHelper;

namespace Tests.Notification.GetListNoti;

public class GetListNotiValidatorTests
{
    private readonly GetListNotiValidator _validator;

    public GetListNotiValidatorTests()
    {
        _validator = new GetListNotiValidator();
    }

    [Fact]
    public void Validator_WhenLimitIsZero_ShouldHaveError()
    {
        var query = new GetListNotiQuery { Limit = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }

    [Fact]
    public void Validator_WhenLimitExceedsMaximum_ShouldHaveError()
    {
        var query = new GetListNotiQuery { Limit = 21 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit từ 0 - 20, không được để trống");
    }

    [Fact]
    public void Validator_WhenLimitIsValid_ShouldNotHaveError()
    {
        var query = new GetListNotiQuery { Limit = 10 };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}