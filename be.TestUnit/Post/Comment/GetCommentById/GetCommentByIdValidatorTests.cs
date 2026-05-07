using be.Application.Features.Post.Comment.GetCommentById;
using FluentValidation.TestHelper;

namespace Tests.Post.Comment.GetCommentById;

public class GetCommentByIdValidatorTests
{
    private readonly GetCommentByIdValidator _validator;

    public GetCommentByIdValidatorTests()
    {
        _validator = new GetCommentByIdValidator();
    }

    [Fact]
    public void Validator_WhenCommentIdIsEmpty_ShouldHaveError()
    {
        var query = new GetCommentByIdQuery { CommentId = Guid.Empty };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.CommentId)
            .WithErrorMessage("CommentId không được để trống");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetCommentByIdQuery { CommentId = Guid.NewGuid() };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}