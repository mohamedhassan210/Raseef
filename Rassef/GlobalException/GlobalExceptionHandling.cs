namespace Rassef.GlobalException
{
    public class GlobalExceptionHandling : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandling> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // التفاصيل الكاملة بتتسجّل في الـ logs برقم مرجعي (TraceId) هو نفسه اللي بيظهر للمستخدم،
            // فالدعم الفني يقدر يلاقي الخطأ بسهولة من غير ما المستخدم يشوف أي حاجة تقنية.
            _logger.LogError(exception,
                "Unhandled exception. TraceId: {TraceId} | {Method} {Path}",
                httpContext.TraceIdentifier, httpContext.Request.Method, httpContext.Request.Path);

            // لو الـ response بدأ يتبعت خلاص مفيش حاجة نقدر نكتبها
            if (httpContext.Response.HasStarted) return false;

            var isAjax = string.Equals(httpContext.Request.Headers["X-Requested-With"], "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase);
            var acceptsHtml = httpContext.Request.Headers.Accept.ToString()
                .Contains("text/html", StringComparison.OrdinalIgnoreCase);

            // 1) المستخدم بيتنقل بين صفحات (متصفح): منرجعش JSON خام. بنسيب الـ pipeline يعرض
            //    صفحة الخطأ الودودة (/Home/Error) اللي معمولة بـ UseExceptionHandler.
            if (acceptsHtml && !isAjax) return false;

            // 2) طلب fetch/AJAX: بنرجّع JSON مختصر بـ message جاهزة تتعرض للمستخدم، من غير
            //    اسم الاستثناء ولا رسالته الداخلية ولا الـ stack trace.
            var isDomain = exception is DomainException;
            var statusCode = isDomain ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;

            var payload = new Dictionary<string, object?>
            {
                ["success"] = false,
                // DomainException رسائلها مكتوبة عشان المستخدم يشوفها (قواعد العمل)؛ أي استثناء تاني = رسالة الدعم
                ["message"] = isDomain ? exception.Message : "حدثت مشكلة غير متوقعة. برجاء التواصل مع الدعم الفني.",
                ["traceId"] = httpContext.TraceIdentifier
            };

            // للمطوّرين فقط (بيظهر في تبويب Network مش في واجهة المستخدم)
            if (_env.IsDevelopment())
            {
                payload["detail"] = exception.Message;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }
    }
}
