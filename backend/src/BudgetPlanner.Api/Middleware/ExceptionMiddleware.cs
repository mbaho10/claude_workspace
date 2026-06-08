using System.Text.Json;

namespace BudgetPlanner.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "เกิดข้อผิดพลาดภายในระบบ กรุณาลองใหม่อีกครั้ง")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var response = JsonSerializer.Serialize(new { message, status });
        return context.Response.WriteAsync(response);
    }
}
