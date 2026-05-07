using FluentValidation;

namespace be.Application.Features.Account.Link.LinkPass;

public class LinkPassValidator : AbstractValidator<LinkPassCommand>
{
    public LinkPassValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Mail)
            .NotEmpty().WithMessage("Mail không để trống")
            .MaximumLength(255).WithMessage("Độ dài email tối đa 255 ký tự")
            .EmailAddress().WithMessage("Định dạng mail không hợp lệ");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP đang trống");

        RuleFor(x => x.Pass)
            .NotEmpty().WithMessage("Mật khẩu đang trống")
            .Length(6, 255).WithMessage("Mật khẩu dài từ 6-255 ký tự");
    }
}