using System.Text;
using System.Text.Json;
using Rassef.Filters;
using Rassef.ViewModels.Administration;

namespace Rassef.Controllers
{
    /// <summary>
    /// نقطة التصدير العامة: أي صفحة فيها جدول وعايزة تصدّره لـ Excel بتبعت عناوين الأعمدة
    /// والصفوف الظاهرة (بعد الفلاتر) ووصف الفلاتر، والسيرفر بيرجّع ملف .xlsx بنفس الـ Header
    /// والـ Footer الموحّدين (العنوان + المخزن + الفلاتر / محرّر التقرير + تاريخ الإصدار).
    ///
    /// أي مستخدم عامل login يقدر يستخدمها (الصفحة نفسها هي اللي بتتحكم في صلاحية عرض بياناتها،
    /// وهنا بنصدّر بس الصفوف اللي المستخدم شافها بالفعل). مفيش أي تسجيل DI مطلوب.
    /// </summary>
    [PermissionAuthorize]
    public class ReportExportController : Controller
    {
        private const int MaxRows = 50000;
        private const int MaxColumns = 60;
        private const int MaxFilterLines = 12;

        private readonly ApplicationDbContext _db;

        public ReportExportController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Table([FromBody] TableExportInput input)
        {
            if (input == null)
                return BadRequest(new { success = false, message = "بيانات التصدير غير صالحة." });

            var title = (input.Title ?? string.Empty).Trim();
            if (title.Length == 0)
                return BadRequest(new { success = false, message = "عنوان التقرير مطلوب." });
            if (title.Length > 120) title = title.Substring(0, 120);

            if (input.Headers == null || input.Headers.Count == 0)
                return BadRequest(new { success = false, message = "لا توجد أعمدة لتصديرها." });
            if (input.Headers.Count > MaxColumns)
                return BadRequest(new { success = false, message = "عدد الأعمدة أكبر من الحد المسموح." });

            var rowsIn = input.Rows ?? new List<List<JsonElement>>();
            if (rowsIn.Count == 0)
                return BadRequest(new { success = false, message = "لا توجد بيانات لتصديرها." });
            if (rowsIn.Count > MaxRows)
                return BadRequest(new { success = false, message = "عدد الصفوف أكبر من الحد المسموح." });

            var headers = input.Headers.Select(h => (h ?? string.Empty).Trim()).ToList();
            var rows = rowsIn.Select(r => (r ?? new List<JsonElement>()).Select(ToCellValue).ToArray()).ToList();

            var filterLines = (input.FilterLines ?? new List<string>())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim().Length > 300 ? l.Trim().Substring(0, 300) : l.Trim())
                .Take(MaxFilterLines)
                .ToList();

            var sheetName = string.IsNullOrWhiteSpace(input.SheetName) ? title : input.SheetName!.Trim();

            var meta = await ExportMetaHelper.BuildAsync(
                HttpContext, _db, title, sheetName, filterLines, input.IncludeWarehouse);

            var bytes = ExcelReportBuilder.Build(meta, headers, rows);
            return File(bytes, ExcelReportBuilder.ContentType, BuildFileName(input.FileName, title));
        }

        private static object? ToCellValue(JsonElement e) => e.ValueKind switch
        {
            JsonValueKind.String => e.GetString(),
            JsonValueKind.Number => e.TryGetInt64(out var l) ? (double)l : e.GetDouble(),
            JsonValueKind.True => "نعم",
            JsonValueKind.False => "لا",
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => e.ToString()
        };

        private static string BuildFileName(string? requested, string title)
        {
            var baseName = string.IsNullOrWhiteSpace(requested) ? title : requested!.Trim();
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (var ch in baseName)
            {
                sb.Append(invalid.Contains(ch) || ch == ' ' ? '_' : ch);
            }
            var cleaned = sb.ToString();
            if (cleaned.Length == 0) cleaned = "Report";
            if (cleaned.Length > 80) cleaned = cleaned.Substring(0, 80);
            return $"{cleaned}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        }
    }
}
