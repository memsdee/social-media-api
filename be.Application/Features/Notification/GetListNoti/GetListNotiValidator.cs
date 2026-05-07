using FluentValidation;

namespace be.Application.Features.Notification.GetListNoti;

public class GetListNotiValidator : AbstractValidator<GetListNotiQuery>
{
    public GetListNotiValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Limit).NotEmpty()
            .GreaterThan(0)
            .LessThanOrEqualTo(20)
            .WithMessage("Limit từ 0 - 20, không được để trống");
    }
}