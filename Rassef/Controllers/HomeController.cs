using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Rassef.Controllers
{
    /// <summary>
    /// صفحة الأخطاء الموحّدة للمستخدم النهائي.
    ///
    /// المستخدم عمره ما يشوف تفاصيل تقنية (استثناءات، رسائل قاعدة بيانات، stack trace): بيشوف رسالة
    /// "حدثت مشكلة، برجاء التواصل مع الدعم الفني" + رقم مرجعي (TraceId) يقدر يقوله للدعم.
    /// التفاصيل الكاملة بتتسجّل بنفس الرقم في ملفات الـ logs (logs/log-*.txt).
    ///
    /// بتتنادى تلقائياً من الـ pipeline في حالتين:
    ///   1) أي استثناء غير متوقع (UseExceptionHandler)
    ///   2) أي 404/403/500... من غير صفحة (UseStatusCodePagesWithReExecute)
    /// </summary>
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class HomeController : Controller
    {
        public const string SupportMessage = "حدثت مشكلة غير متوقعة. برجاء التواصل مع الدعم الفني.";

        // بتتنادى بنفس HTTP method الأصلي للطلب (ممكن يكون POST) فلازم تقبل أكتر من method
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH")]
        public IActionResult Error(int? code)
        {
            var status = code ?? Response.StatusCode;
            if (status < 400 || status > 599) status = (int)HttpStatusCode.InternalServerError;

            var message = status switch
            {
                404 => "الصفحة التي تبحث عنها غير موجودة.",
                403 => "ليس لديك صلاحية للوصول إلى هذه الصفحة.",
                _ => SupportMessage
            };

            var title = status switch
            {
                404 => "الصفحة غير موجودة",
                403 => "غير مصرح بالدخول",
                _ => "حدثت مشكلة"
            };

            Response.StatusCode = status;

            // طلبات fetch/AJAX (مفيهاش text/html في Accept) بتاخد JSON مختصر بدل صفحة كاملة
            var acceptsHtml = Request.Headers.Accept.ToString()
                .Contains("text/html", StringComparison.OrdinalIgnoreCase);
            if (!acceptsHtml)
            {
                return Json(new { success = false, message, traceId = HttpContext.TraceIdentifier });
            }

            ViewBag.StatusCode = status;
            ViewBag.ErrorTitle = title;
            ViewBag.ErrorMessage = message;
            ViewBag.TraceId = HttpContext.TraceIdentifier;
            return View();
        }
    }
}
