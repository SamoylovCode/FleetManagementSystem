using Serilog.Context;

namespace FleetManagementSystemApp.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Если пришёл извне X-Correlation-ID, используем его; иначе — генерируем
        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var cId))
        {
            cId = Guid.NewGuid().ToString();
            context.Request.Headers["X-Correlation-ID"] = cId;
        }
        // 2. PushProperty в LogContext
        using (LogContext.PushProperty("CorrelationId", cId.ToString()))
        {
            // также задаём TraceIdentifier явно, чтобы Serilog.Enrichers.CorrelationId не путался
            context.TraceIdentifier = cId.ToString();
            await _next(context);
        }
    }
}
