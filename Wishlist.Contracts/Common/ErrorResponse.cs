namespace Wishlist.Contracts.Common;

public record ErrorResponse(string TraceId, ErrorDetail Error);

public record ErrorDetail(string Code, string Message, IDictionary<string, string[]>? Details = null);
