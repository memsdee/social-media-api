using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Search.SearchAll;

public class SearchAllQuery : IRequest<BaseResponse<SearchAllResponse>>
{
    public string Q
    {
        get;
        set => field = value.Trim().ToLower();
    } = string.Empty;

    public short LimitUser { get; set; }
    public short LimitPost { get; set; }
}