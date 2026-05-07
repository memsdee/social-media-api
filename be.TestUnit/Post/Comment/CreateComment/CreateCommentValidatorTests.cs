using be.Application.Features.Post.Comment.CreateComment;
using FluentValidation.TestHelper;

namespace Tests.Post.Comment.CreateComment;

public class CreateCommentValidatorTests
{
    private readonly CreateCommentValidator _validator;

    public CreateCommentValidatorTests()
    {
        _validator = new CreateCommentValidator();
    }

    [Fact]
    public void Validator_WhenPostIdIsEmpty_ShouldHaveError()
    {
        var command = new CreateCommentCommand { PostId = Guid.Empty, Comment = "Hello" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId không để trống");
    }

    [Fact]
    public void Validator_WhenCommentIsEmpty_ShouldHaveError()
    {
        var command = new CreateCommentCommand { PostId = Guid.NewGuid(), Comment = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Nội dung bình luận không để trống");
    }

    [Fact]
    public void Validator_WhenCommentExceedsMaximumLength_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            PostId = Guid.NewGuid(),
            Comment = new string('a', 1001)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Nội dung bình luận không vượt quá 1000 ký tự");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new CreateCommentCommand { PostId = Guid.NewGuid(), Comment = "Hello" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}