using be.Application.Features.Account.Link.LinkGoogle;
using FluentValidation.TestHelper;

namespace Tests.Account.Link.LinkGoogle;

public class LinkGoogleValidatorTests
{
    private readonly LinkGoogleValidator _validator;

    public LinkGoogleValidatorTests()
    {
        _validator = new LinkGoogleValidator();
    }

    [Fact]
    public void Validator_WhenCodeIsEmpty_ShouldHaveError()
    {
        var command = new LinkGoogleCommand { Code = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code đang trống");
    }

    [Fact]
    public void Validator_WhenCodeIsNotEmpty_ShouldNotHaveError()
    {
        var command = new LinkGoogleCommand { Code = "valid_code" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }
}