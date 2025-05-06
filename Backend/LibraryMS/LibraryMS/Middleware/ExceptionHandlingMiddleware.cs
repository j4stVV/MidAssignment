using Lib.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace LibraryManament.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = new ErrorResponse
        {
            Error = "An unexpected error occurred.",
            Details = new List<string>()
        };
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case RegisterValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                response.Error = "Validation failed.";
                response.Details = validationEx.Errors;
                break;

            case FluentValidation.ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                response.Error = "Validation failed.";
                response.Details = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                response.Error = notFoundEx.Message;
                break;

            case BusinessRuleException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                response.Error = businessEx.Message;
                break;

            case KeyNotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                response.Error = notFoundEx.Message;
                break;

            case UnauthorizedAccessException unauthorizedEx:
                statusCode = HttpStatusCode.BadRequest;
                response.Error = unauthorizedEx.Message;
                break;

            case InvalidOperationException invalidOpEx:
                statusCode = HttpStatusCode.BadRequest;
                response.Error = invalidOpEx.Message;
                break;

            default:
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
