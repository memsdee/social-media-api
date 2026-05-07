using FluentValidation;

namespace be.Application.Features.Post.Post.GetListPost;

public class GetListPostValidator : AbstractValidator<GetListPostQuery>
{
    public GetListPostValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.TargetId)
            .Length(3, 50).WithMessage("Độ dài userid từ 3-50 ký tự")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("UserId chỉ được chứa chữ cái, số và dấu gạch dưới");

        RuleFor(x => x.Limit)
            .NotEmpty().WithMessage("Limit không được trống")
            .InclusiveBetween((short)1, (short)50).WithMessage("Limit từ 1-50");

        When(x => !string.IsNullOrEmpty(x.TargetId), () =>
        {
            RuleFor(x => x.Tab)
                .Must(x => x == null)
                .WithMessage("Khi có targetId thì tab phải là rỗng");
        });
    }
}