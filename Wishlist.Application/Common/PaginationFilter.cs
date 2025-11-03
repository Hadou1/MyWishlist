namespace Wishlist.Application.Common;

public class PaginationFilter
{
    private const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }

    public PaginationFilter(int? page, int? pageSize)
    {
        Page = page is > 0 ? page.Value : 1;
        PageSize = pageSize is > 0 ? Math.Min(pageSize.Value, MaxPageSize) : 20;
    }
}
