using be.Application.Features.Post.Post.DeletePost;
using FluentValidation.TestHelper;

namespace Tests.Post.Post.DeletePost;

public class DeletePostValidatorTests
{
    private readonly DeletePostValidator _validator = new();

    [Fact]
    public void Validator_WhenPostIdEmpty_ShouldHaveError()
    {
        var command = new DeletePostCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("Postid không để trống");
    }

    [Fact]
    public void Validator_WhenPostIdValid_ShouldNotHaveError()
    {
        var command = new DeletePostCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}