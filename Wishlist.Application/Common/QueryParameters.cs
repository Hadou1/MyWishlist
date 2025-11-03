namespace Wishlist.Application.Common;

public class QueryParameters
{
    public PaginationFilter Pagination { get; }
    public string? Search { get; }
    public string? Sort { get; }

    public QueryParameters(int? page, int? pageSize, string? sort, string? search)
    {
        Pagination = new PaginationFilter(page, pageSize);
        Sort = sort;
        Search = search;
    }
}
