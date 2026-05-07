using be.Application.Features.Post.Post.ReportPost;
using FluentValidation.TestHelper;

namespace Tests.Post.Post.ReportPost;

public class ReportPostValidatorTests
{
    private readonly ReportPostValidator _validator = new();

    [Fact]
    public void Validator_WhenPostIdEmpty_ShouldHaveError()
    {
        var command = new ReportPostCommand { PostId = Guid.Empty, ReasonId = [1] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId không để trống");
    }

    [Fact]
    public void Validator_WhenReasonIdEmpty_ShouldHaveError()
    {
        var command = new ReportPostCommand { PostId = Guid.NewGuid(), ReasonId = [] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ReasonId)
            .WithErrorMessage("Phải có ít nhất một lý do báo cáo");
    }

    [Fact]
    public void Validator_WhenReasonIdNegative_ShouldHaveError()
    {
        var command = new ReportPostCommand { PostId = Guid.NewGuid(), ReasonId = [-1] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("ReasonId[0]")
            .WithErrorMessage("Lý do phải >0");
    }

    [Fact]
    public void Validator_WhenOtherReasonTooLong_ShouldHaveError()
    {
        var command = new ReportPostCommand
        {
            PostId = Guid.NewGuid(),
            ReasonId = [1],
            OtherReason = new string('a', 1001)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtherReason)
            .WithErrorMessage("Lý do `khác` dài tối đa 1000 ký tự");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new ReportPostCommand { PostId = Guid.NewGuid(), ReasonId = [1] };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}