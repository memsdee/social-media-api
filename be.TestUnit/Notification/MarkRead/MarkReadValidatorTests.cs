using be.Application.Features.Notification.MarkRead;
using FluentValidation.TestHelper;

namespace Tests.Notification.MarkRead;

public class MarkReadValidatorTests
{
    private readonly MarkReadValidator _validator;

    public MarkReadValidatorTests()
    {
        _validator = new MarkReadValidator();
    }

    [Fact]
    public void Validator_WhenEncryptedIdsIsEmpty_ShouldHaveError()
    {
        var command = new MarkReadCommand { EncryptedIds = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EncryptedIds)
            .WithErrorMessage("EncryptedIds không được để trống");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new MarkReadCommand { EncryptedIds = "some-encrypted-string" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}