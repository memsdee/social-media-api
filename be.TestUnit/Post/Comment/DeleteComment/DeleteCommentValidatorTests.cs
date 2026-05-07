using be.Application.Features.Post.Comment.DeleteComment;
using FluentValidation.TestHelper;

namespace Tests.Post.Comment.DeleteComment;

public class DeleteCommentValidatorTests
{
    private readonly DeleteCommentValidator _validator;

    public DeleteCommentValidatorTests()
    {
        _validator = new DeleteCommentValidator();
    }

    [Fact]
    public void Validator_WhenIdPublicEmpty_ShouldHaveError()
    {
        var command = new DeleteCommentCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IdPublic)
            .WithErrorMessage("Comment ID không để trống");
    }

    [Fact]
    public void Validator_WhenIdPublicValid_ShouldNotHaveError()
    {
        var command = new DeleteCommentCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}