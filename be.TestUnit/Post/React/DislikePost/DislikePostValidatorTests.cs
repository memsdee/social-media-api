using be.Application.Features.Post.React.DislikePost;
using FluentValidation.TestHelper;

namespace Tests.Post.React.DislikePost;

public class DislikePostValidatorTests
{
    private readonly DislikePostValidator _validator = new();

    [Fact]
    public void Validator_WhenPostIdEmpty_ShouldHaveError()
    {
        var command = new DislikePostCommand { PostId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void Validator_WhenPostIdValid_ShouldNotHaveError()
    {
        var command = new DislikePostCommand { PostId = Guid.NewGuid() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}