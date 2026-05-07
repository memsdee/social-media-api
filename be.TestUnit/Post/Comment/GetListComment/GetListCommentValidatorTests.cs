using be.Application.Features.Post.Comment.GetListComment;
using FluentValidation.TestHelper;

namespace Tests.Post.Comment.GetListComment;

public class GetListCommentValidatorTests
{
    private readonly GetListCommentValidator _validator = new();

    [Fact]
    public void Validator_WhenTargetIdEmpty_ShouldHaveError()
    {
        var query = new GetListCommentQuery { TargetId = Guid.Empty, Limit = 10 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TargetId)
            .WithErrorMessage("PostId không để trống");
    }

    [Fact]
    public void Validator_WhenLimitTooLow_ShouldHaveError()
    {
        var query = new GetListCommentQuery { TargetId = Guid.NewGuid(), Limit = 0 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit không để trống");
    }

    [Fact]
    public void Validator_WhenLimitTooHigh_ShouldHaveError()
    {
        var query = new GetListCommentQuery { TargetId = Guid.NewGuid(), Limit = 51 };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit)
            .WithErrorMessage("Limit từ 1-50");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetListCommentQuery { TargetId = Guid.NewGuid(), Limit = 10 };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}