using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace TaskManagementAPI.Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware for structured HTTP request/response logging via Serilog.
/// Enriches every log line in the request scope with RequestId, UserId,
/// HTTP Method, Path, and response duration — enabling full correlation
/// across all log sinks (console, file, audit).
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers.TryGetValue("X-Request-ID", out var header)
            ? header.ToString()
            : context.TraceIdentifier;

        var userId = context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "anonymous";

        var method = context.Request.Method;
        var path   = context.Request.Path.Value ?? "/";
        var ip     = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Push properties into Serilog's real LogContext — every log statement
        // emitted within this using block will carry these enrichment keys.
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserId",    userId))
        using (LogContext.PushProperty("ClientIp",  ip))
        {
            var sw = Stopwatch.StartNew();

            Log.Information(
                "→ {Method} {Path} | IP={ClientIp} | User={UserId} | Req={RequestId}",
                method, path, ip, userId, requestId);

            try
            {
                await _next(context);

                sw.Stop();

                var statusCode = context.Response.StatusCode;
                var level      = statusCode >= 500 ? Serilog.Events.LogEventLevel.Error
                               : statusCode >= 400 ? Serilog.Events.LogEventLevel.Warning
                               :                     Serilog.Events.LogEventLevel.Information;

                Log.Write(level,
                    "← {Method} {Path} | {StatusCode} | {ElapsedMs}ms | User={UserId}",
                    method, path, statusCode, sw.ElapsedMilliseconds, userId);
            }
            catch (Exception ex)
            {
                sw.Stop();

                Log.Error(ex,
                    "✗ {Method} {Path} | EXCEPTION after {ElapsedMs}ms | User={UserId}",
                    method, path, sw.ElapsedMilliseconds, userId);

                throw;
            }
        }
    }
}
