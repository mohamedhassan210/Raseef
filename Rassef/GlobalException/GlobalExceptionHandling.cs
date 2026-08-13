
namespace Rassef.GlobalException
{
    public class GlobalExceptionHandling : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandling> _logger;
        public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger)
         => _logger = logger;
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception occured . Path :{Path}", httpContext.Request.Path);


            var statusCode = exception switch
            {
                DomainException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError

            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status400BadRequest ? "Bad Request" : "Internal Server Error",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
