using System.Diagnostics;

namespace Rassef.Piplines
{
    public class LoggingBehavior
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingBehavior> _logger;
        public LoggingBehavior(RequestDelegate request, ILogger<LoggingBehavior> logger)
        {
            _next = request;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var userName = context.User?.Identity?.Name ?? "Anonymous";

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled Exception | Path: {Path}",
                    context.Request.Path);

                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Method: {Method} | Path: {Path} | StatusCode: {StatusCode} | Time: {Elapsed} ms | IP: {IP} | Query String: {QueryString} | User : {User}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.Connection.RemoteIpAddress,
                    context.Request.QueryString,
                    userName);
            }
        }
    }
}
