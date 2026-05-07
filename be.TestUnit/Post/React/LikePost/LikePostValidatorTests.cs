using be.Application.Features.Post.React.LikePost;
using FluentValidation.TestHelper;

namespace Tests.Post.React.LikePost;

public class LikePostValidatorTests
{
    private readonly LikePostValidator _validator = new();

    [Fact]
    public void Validator_WhenPostIdEmpty_ShouldHaveError()
    {
        var command = new LikePostCommand { PostId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void Validator_WhenPostIdValid_ShouldNotHaveError()
    {
        var command = new LikePostCommand { PostId = Guid.NewGuid() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}