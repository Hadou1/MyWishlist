namespace Wishlist.Contracts.Common;

public record PagedResult<T>(IReadOnlyCollection<T> Data, int Page, int PageSize, long Total);
