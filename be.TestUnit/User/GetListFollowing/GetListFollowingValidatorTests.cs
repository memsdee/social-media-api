using be.Application.Features.User.GetListFollowing;
using FluentValidation.TestHelper;

namespace Tests.User.GetListFollowing;

public class GetListFollowingValidatorTests
{
    private readonly GetListFollowingValidator _validator = new();

    [Fact]
    public void Validator_WhenTargetIdIsEmpty_ShouldHaveError()
    {
        var command = new GetListFollowingQuery("", null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("TargetId không được trống");
    }

    [Fact]
    public void Validator_WhenLimitIsZero_ShouldHaveError()
    {
        var command = new GetListFollowingQuery("target-123", null, 0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit phải từ 1 đến 50");
    }

    [Fact]
    public void Validator_WhenLimitIsTooHigh_ShouldHaveError()
    {
        var command = new GetListFollowingQuery("target-123", null, 21);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit phải từ 1 đến 50");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new GetListFollowingQuery("target-123", null, 10);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}