using System.Diagnostics;

namespace Rassef.Piplines
{
    public class LogginBehaviors
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogginBehaviors> _logger;
        public LogginBehaviors(RequestDelegate request, ILogger<LogginBehaviors> logger)
        {
            _next = request;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var startr = Stopwatch.StartNew();

            await _next(context);

            _logger.LogInformation("Method : {Method} | Path : {Path} | StatusCode : {StatusCode} | Time : { Elapsed} ms | IP : {IP} "
            , context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            startr.ElapsedMilliseconds,
            context.Connection.RemoteIpAddress
                );

        }
    }
}
