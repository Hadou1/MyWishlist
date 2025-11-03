using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Wishlist.Application.Models;
using Wishlist.Contracts.Common;

namespace Wishlist.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationAppException validationException)
        {
            await WriteErrorAsync(context, validationException.StatusCode, validationException.ErrorCode, validationException.Message, validationException.Errors);
        }
        catch (AppException appException)
        {
            await WriteErrorAsync(context, appException.StatusCode, appException.ErrorCode, appException.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "server.error", "An unexpected error occurred", null);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string code, string message, IDictionary<string, string[]>? details)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse(context.TraceIdentifier, new ErrorDetail(code, message, details));
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
