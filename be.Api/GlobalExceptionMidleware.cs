using be.Application.Dtos.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using static be.Domain.CustomException;

namespace be.Api;

public class GlobalExceptionMidleware(ILogger<GlobalExceptionMidleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            ValidationException ex => (StatusCodes.Status400BadRequest,
                string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BusinessValidationException => (StatusCodes.Status422UnprocessableEntity, exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, exception.Message),
            TooManyRequestsException => (StatusCodes.Status429TooManyRequests, exception.Message),
            ArgumentNullException or ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Có gì đó nhầm lẫn, vui lòng liên hệ quản trị viên")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = exception is BusinessValidationException bve && bve.ValidationData != null
            ? new { message, data = bve.ValidationData }
            : (object)new BaseResponse { Message = message };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}