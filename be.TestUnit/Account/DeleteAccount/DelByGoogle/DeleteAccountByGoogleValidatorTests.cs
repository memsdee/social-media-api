using be.Application.Features.Account.DeleteAccount.DelByGoogle;
using FluentValidation.TestHelper;

namespace Tests.Account.DeleteAccount.DelByGoogle;

public class DeleteAccountByGoogleValidatorTests
{
    private readonly DelByGoogleValidator _validator;

    public DeleteAccountByGoogleValidatorTests()
    {
        _validator = new DelByGoogleValidator();
    }

    [Fact]
    public void Should_Have_Error_When_GoogleCode_Is_Empty()
    {
        var command = new DelByGoogleCommand { GoogleCode = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.GoogleCode)
            .WithErrorMessage("Vui lòng xác thực lại Google để tiếp tục");
    }

    [Fact]
    public void Should_Have_Error_When_GoogleCode_Exceeds_Maximum_Length()
    {
        var command = new DelByGoogleCommand { GoogleCode = new string('a', 2049) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.GoogleCode)
            .WithErrorMessage("Google code quá dài");
    }

    [Fact]
    public void Should_Not_Have_Error_When_GoogleCode_Is_Valid()
    {
        var command = new DelByGoogleCommand { GoogleCode = "valid_google_code" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.GoogleCode);
    }
}