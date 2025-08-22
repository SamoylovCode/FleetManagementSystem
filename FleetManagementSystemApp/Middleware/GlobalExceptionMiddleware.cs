using Serilog;
using System.Net;

namespace FleetManagementSystemApp.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Собираем общий CorrelationId (если нужно, можно взять из LogContext)
            var correlationId = context.TraceIdentifier;

            // Логируем исключение с полным стеком
            // Уровень Error (или Critical, если приложение «умирает»)
            Log.Logger.ForContext("CorrelationId", correlationId).
                Error(
                    ex,
                    "Unhandled exception. CorrelationId={CorrelationId}, RequestPath={RequestPath}",
                    correlationId,
                    context.Request.Path);

            // Формируем ответ клиенту (JSON)
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Message = "Внутренняя ошибка сервера. Пожалуйста, повторите попытку позже.",
                CorrelationId = correlationId
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}