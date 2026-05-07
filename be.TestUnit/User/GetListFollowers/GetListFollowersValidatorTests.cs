using be.Application.Features.User.GetListFollowers;
using FluentValidation.TestHelper;

namespace Tests.User.GetListFollowers;

public class GetListFollowersValidatorTests
{
    private readonly GetListFollowersValidator _validator = new();

    [Fact]
    public void Validator_WhenTargetIdEmpty_ShouldHaveError()
    {
        var query = new GetListFollowersQuery("", null, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("TargetId không được trống");
    }

    [Fact]
    public void Validator_WhenLimitZero_ShouldHaveError()
    {
        var query = new GetListFollowersQuery("target", null, 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit không được trống");
    }

    [Fact]
    public void Validator_WhenLimitTooHigh_ShouldHaveError()
    {
        var query = new GetListFollowersQuery("target", null, 21);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetListFollowersQuery("target", null, 15);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}