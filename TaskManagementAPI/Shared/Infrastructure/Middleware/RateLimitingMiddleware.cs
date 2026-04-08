using System.Collections.Concurrent;

namespace TaskManagementAPI.Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware for implementing rate limiting to prevent abuse and DoS attacks.
/// Uses a sliding window approach with configurable requests per time window.
/// Includes automatic cleanup of stale entries to prevent memory leaks.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerWindow;
    private readonly TimeSpan _timeWindow;
    private static readonly ConcurrentDictionary<string, ClientRateLimitData> _requestHistory = new();
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the RateLimitingMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="requestsPerWindow">Maximum requests allowed per time window (default: 100).</param>
    /// <param name="timeWindowSeconds">Time window in seconds (default: 60).</param>
    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, int requestsPerWindow = 100, int timeWindowSeconds = 60)
    {
        _next = next;
        _logger = logger;
        _requestsPerWindow = requestsPerWindow;
        _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
    }

    /// <summary>
    /// Invokes the middleware to check rate limits for the current request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (!IsRequestAllowed(clientId))
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded. Please try again later." });
            return;
        }

        // Perform cleanup if needed
        if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
        {
            CleanupStaleEntries();
            _lastCleanup = DateTime.UtcNow;
        }

        await _next(context);
    }

    /// <summary>
    /// Gets a unique identifier for the client (IP address or user ID).
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A unique client identifier.</returns>
    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims first
        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    /// <summary>
    /// Checks if a request from the given client is allowed based on rate limits.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>True if the request is allowed; otherwise false.</returns>
    private bool IsRequestAllowed(string clientId)
    {
        var now = DateTime.UtcNow;
        var data = _requestHistory.AddOrUpdate(clientId,
            new ClientRateLimitData { Requests = new Queue<DateTime>() },
            (key, existing) => existing);

        lock (data)
        {
            // Remove old requests outside the time window
            while (data.Requests.Count > 0 && data.Requests.Peek() < now - _timeWindow)
            {
                data.Requests.Dequeue();
            }

            // Check if limit is exceeded
            if (data.Requests.Count >= _requestsPerWindow)
                return false;

            // Add current request
            data.Requests.Enqueue(now);
            data.LastActivity = now;
            return true;
        }
    }

    /// <summary>
    /// Cleans up stale entries from the rate limit dictionary to prevent memory leaks.
    /// Removes entries that haven't been accessed in the last hour.
    /// </summary>
    private static void CleanupStaleEntries()
    {
        var now = DateTime.UtcNow;
        var staleThreshold = TimeSpan.FromHours(1);
        var staleEntries = _requestHistory
            .Where(kvp => now - kvp.Value.LastActivity > staleThreshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var entry in staleEntries)
        {
            _requestHistory.TryRemove(entry, out _);
        }
    }

    /// <summary>
    /// Represents rate limit data for a client.
    /// </summary>
    private class ClientRateLimitData
    {
        public Queue<DateTime> Requests { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}
