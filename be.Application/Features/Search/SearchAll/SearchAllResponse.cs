using be.Application.Dtos.Pagination;
using be.Application.Features.Post;

namespace be.Application.Features.Search.SearchAll;

public class SearchAllResponse
{
    public List<UerSearchDto> User { get; set; } = [];
    public List<PostResponse> Post { get; set; } = [];
    public PagiResult PageProfileUser { get; set; } = null!;
    public PagiResult PageProfilePost { get; set; } = null!;
}