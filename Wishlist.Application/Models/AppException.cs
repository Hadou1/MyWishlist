using System.Net;

namespace Wishlist.Application.Models;

public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }

    public AppException(HttpStatusCode statusCode, string errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string errorCode, string message) : base(HttpStatusCode.NotFound, errorCode, message)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string errorCode, string message) : base(HttpStatusCode.Forbidden, errorCode, message)
    {
    }
}

public class ValidationAppException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(string errorCode, string message, IDictionary<string, string[]> errors)
        : base(HttpStatusCode.BadRequest, errorCode, message)
    {
        Errors = errors;
    }
}
