using be.Application.Features.Search.SearchAccount;
using FluentValidation.TestHelper;

namespace Tests.Search.SearchAccount;

public class SearchAccountValidatorTests
{
    private readonly SearchAccountValidator _validator = new();

    [Fact]
    public void Validator_WhenQIsEmpty_ShouldHaveError()
    {
        var query = new SearchAccountQuery { Q = "", Limit = 10 };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Q)
            .WithErrorMessage("Param đang trống");
    }

    [Fact]
    public void Validator_WhenLimitIsZero_ShouldHaveError()
    {
        var query = new SearchAccountQuery { Q = "test", Limit = 0 };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit không được để trống");
    }

    [Fact]
    public void Validator_WhenLimitTooHigh_ShouldHaveError()
    {
        var query = new SearchAccountQuery { Q = "test", Limit = 51 };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit từ 1-50");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new SearchAccountQuery { Q = "test", Limit = 10 };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}