using be.Application.Features.User.Follow;
using FluentValidation.TestHelper;

namespace Tests.User.Follow;

public class FollowValidatorTests
{
    private readonly FollowValidator _validator = new();

    [Fact]
    public void Validator_WhenUserIdEmpty_ShouldHaveError()
    {
        var command = new FollowCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserIdToFollow);
    }

    [Fact]
    public void Validator_WhenUserIdTooShort_ShouldHaveError()
    {
        var command = new FollowCommand("ab");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserIdToFollow);
    }

    [Fact]
    public void Validator_WhenUserIdTooLong_ShouldHaveError()
    {
        var command = new FollowCommand(new string('a', 51));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserIdToFollow);
    }

    [Fact]
    public void Validator_WhenUserIdHasInvalidChars_ShouldHaveError()
    {
        var command = new FollowCommand("user-1");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserIdToFollow);
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new FollowCommand("user_123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}