using System.Globalization;
using System.Security.Claims;

namespace Rassef.Common.Interfaces.Services.FileServices
{
    /// <summary>
    /// بيانات الـ Header والـ Footer اللي بتظهر جوه أي ملف Excel بيتصدّر من المشروع.
    /// </summary>
    public class ExcelReportMeta
    {
        /// <summary>عنوان التقرير، مثلاً: طلبات تحويل</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>اسم الورقة (Sheet) في الملف</summary>
        public string SheetName { get; set; } = "Report";

        /// <summary>اسم المخزن الحالي (اختياري)</summary>
        public string? WarehouseName { get; set; }

        /// <summary>سطور الفلاتر المطبّقة (شهر / فترة / قسم / حالة / بحث). تتكتب بس لو فيه فلتر فعلاً.</summary>
        public List<string> FilterLines { get; set; } = new();

        /// <summary>اسم الشخص اللي حرّر التقرير (المستخدم اللي عامل login)</summary>
        public string IssuedBy { get; set; } = string.Empty;

        /// <summary>تاريخ ووقت إصدار التقرير</summary>
        public DateTime IssuedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// بيبني ملف Excel لأي تقرير في المشروع بنفس الشكل:
    /// Header (العنوان + المخزن + الفلاتر) في أول الشيت، الجدول، وبعدها Footer
    /// (عدد السجلات + محرّر التقرير + تاريخ الإصدار)، وكمان Header/Footer الطباعة.
    ///
    /// CLASS ثابتة (static) ومفيهاش أي اعتماد على الـ DI عن قصد: التصدير كان بيقع
    /// سابقاً لأن خدمة التقرير كانت لازم تتسجل في DependencyInjection.cs، ولما التسجيل
    /// ده اتمسح بالغلط (استبدال الملف بنسخة قديمة) كل طلبات التصدير بقت بترجع 500.
    /// دلوقتي مفيش حاجة تتسجل أصلاً، فالمشكلة دي مينفعش تتكرر.
    /// </summary>
    public static class ExcelReportBuilder
    {
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private const string HeaderFill = "#E29547";
        private const string TitleFill = "#EB842D";
        private const string SoftFill = "#FDF1E7";
        private const int MaxCellLength = 32000; // حد Excel للخلية الواحدة 32767 حرف

        public static byte[] BuildFromStrings(ExcelReportMeta meta, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
        {
            var objectRows = rows.Select(r => r.Cast<object?>().ToArray()).ToList();
            return Build(meta, headers, objectRows);
        }

        public static byte[] Build(ExcelReportMeta meta, IReadOnlyList<string> headers, IReadOnlyList<object?[]> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(SafeSheetName(meta.SheetName));
            ws.RightToLeft = true;

            int colCount = Math.Max(headers.Count, 2);
            int r = 1;

            // ===================== Header =====================
            r = WriteMergedLine(ws, r, colCount, meta.Title, 18, bold: true, fill: TitleFill, fontColor: "#FFFFFF");

            if (!string.IsNullOrWhiteSpace(meta.WarehouseName))
            {
                r = WriteMergedLine(ws, r, colCount, $"المخزن: {meta.WarehouseName}", 13, bold: true, fill: SoftFill);
            }

            foreach (var line in meta.FilterLines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                r = WriteMergedLine(ws, r, colCount, line, 12, bold: false, fill: SoftFill);
            }

            r++; // سطر فاصل

            // ===================== Table =====================
            int headerRow = r;
            for (int c = 0; c < headers.Count; c++)
            {
                ws.Cell(headerRow, c + 1).Value = headers[c];
            }

            var headerRange = ws.Range(headerRow, 1, headerRow, headers.Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderFill);
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int dataRow = headerRow + 1;
            foreach (var row in rows)
            {
                for (int c = 0; c < headers.Count; c++)
                {
                    SetCell(ws.Cell(dataRow, c + 1), c < row.Length ? row[c] : null);
                }
                dataRow++;
            }

            int lastDataRow = dataRow - 1;
            if (lastDataRow >= headerRow + 1)
            {
                var dataRange = ws.Range(headerRow + 1, 1, lastDataRow, headers.Count);
                dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRange.Style.Alignment.WrapText = true;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // عرض الأعمدة بناءً على الجدول بس (الـ Header والـ Footer مدموجين فمش بيأثروا)
            ws.Columns(1, headers.Count).AdjustToContents(headerRow, Math.Max(lastDataRow, headerRow));
            for (int c = 1; c <= headers.Count; c++)
            {
                var col = ws.Column(c);
                if (col.Width < 12) col.Width = 12;
                if (col.Width > 40) col.Width = 40;
            }

            // ===================== Footer =====================
            r = Math.Max(lastDataRow, headerRow) + 2;
            r = WriteMergedLine(ws, r, colCount, $"عدد السجلات: {rows.Count}", 12, bold: true, fill: SoftFill);
            r = WriteMergedLine(ws, r, colCount, $"حُرّر بواسطة: {meta.IssuedBy}", 12, bold: true, fill: SoftFill);
            r = WriteMergedLine(ws, r, colCount, $"تاريخ إصدار التقرير: {FormatDateTime(meta.IssuedAt)}", 12, bold: false, fill: SoftFill);

            // تثبيت صف عناوين الأعمدة
            ws.SheetView.FreezeRows(headerRow);

            // ===================== Print header / footer =====================
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);
            ws.PageSetup.SetRowsToRepeatAtTop(headerRow, headerRow);

            var printTitle = string.IsNullOrWhiteSpace(meta.WarehouseName)
                ? meta.Title
                : $"{meta.Title} - {meta.WarehouseName}";
            ws.PageSetup.Header.Center.AddText(EscapeHeaderFooter(printTitle), XLHFOccurrence.AllPages);
            ws.PageSetup.Footer.Right.AddText(EscapeHeaderFooter($"حُرّر بواسطة: {meta.IssuedBy}"), XLHFOccurrence.AllPages);
            ws.PageSetup.Footer.Center.AddText(XLHFPredefinedText.PageNumber, XLHFOccurrence.AllPages);
            ws.PageSetup.Footer.Left.AddText(EscapeHeaderFooter(FormatDateTime(meta.IssuedAt)), XLHFOccurrence.AllPages);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>الأرقام بتتكتب كأرقام، والنصوص كنصوص (الأرقام الطويلة أو اللي بتبدأ بصفر زي الهاتف والرقم القومي بتتبعت كنصوص من الأصل).</summary>
        private static void SetCell(IXLCell cell, object? value)
        {
            switch (value)
            {
                case null:
                    break;
                case string s:
                    cell.Value = s.Length > MaxCellLength ? s.Substring(0, MaxCellLength) : s;
                    break;
                case double d:
                    cell.Value = d;
                    break;
                case int i:
                    cell.Value = (double)i;
                    break;
                case long l:
                    cell.Value = (double)l;
                    break;
                case decimal m:
                    cell.Value = (double)m;
                    break;
                case bool b:
                    cell.Value = b ? "نعم" : "لا";
                    break;
                case DateTime dt:
                    cell.Value = dt;
                    break;
                default:
                    cell.Value = value.ToString() ?? string.Empty;
                    break;
            }
        }

        private static int WriteMergedLine(IXLWorksheet ws, int row, int colCount, string text, double fontSize,
            bool bold, string? fill = null, string? fontColor = null)
        {
            ws.Cell(row, 1).Value = text;
            var range = ws.Range(row, 1, row, colCount);
            range.Merge();
            range.Style.Font.FontSize = fontSize;
            range.Style.Font.Bold = bold;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            if (!string.IsNullOrEmpty(fill)) range.Style.Fill.BackgroundColor = XLColor.FromHtml(fill);
            if (!string.IsNullOrEmpty(fontColor)) range.Style.Font.FontColor = XLColor.FromHtml(fontColor);
            ws.Row(row).Height = fontSize >= 16 ? 30 : 22;
            return row + 1;
        }

        private static string FormatDateTime(DateTime value) =>
            value.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);

        // '&' في Header/Footer الطباعة له معنى خاص في Excel
        private static string EscapeHeaderFooter(string text) => text.Replace("&", "&&");

        private static string SafeSheetName(string name)
        {
            var invalid = new[] { '[', ']', ':', '*', '?', '/', '\\' };
            var cleaned = new string((string.IsNullOrWhiteSpace(name) ? "Report" : name)
                .Where(ch => !invalid.Contains(ch)).ToArray());
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "Report";
            return cleaned.Length > 31 ? cleaned.Substring(0, 31) : cleaned;
        }
    }

    /// <summary>
    /// بيجمّع بيانات الـ Header/Footer الثابتة لأي تصدير: المخزن الحالي والمستخدم اللي عامل login.
    /// بيعتمد على ApplicationDbContext بس (متسجّل أصلاً في كل المشروع) — مفيش تسجيل جديد مطلوب.
    /// </summary>
    public static class ExportMetaHelper
    {
        public static async Task<ExcelReportMeta> BuildAsync(
            HttpContext http,
            ApplicationDbContext db,
            string title,
            string sheetName,
            IEnumerable<string>? filterLines = null,
            bool includeWarehouse = true)
        {
            User? user = null;
            var idStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.User.FindFirstValue("sub");
            if (int.TryParse(idStr, out var userId))
            {
                user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            }

            string? warehouseName = null;
            if (includeWarehouse)
            {
                int? warehouseId = null;
                if (http.Request.Cookies.TryGetValue("SelectedWarehouseId", out var cookieValue)
                    && int.TryParse(cookieValue, out var parsed))
                {
                    warehouseId = parsed;
                }
                warehouseId ??= user?.LastPickedWarehouseId;

                if (warehouseId.HasValue)
                {
                    var warehouse = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == warehouseId.Value);
                    warehouseName = string.IsNullOrWhiteSpace(warehouse?.Name) ? null : warehouse!.Name;
                }
            }

            string issuedBy = "غير معروف";
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.Name)) issuedBy = user.Name;
                else if (!string.IsNullOrWhiteSpace(user.UserName)) issuedBy = user.UserName!;
            }

            return new ExcelReportMeta
            {
                Title = title,
                SheetName = sheetName,
                WarehouseName = warehouseName,
                FilterLines = (filterLines ?? Enumerable.Empty<string>()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList(),
                IssuedBy = issuedBy,
                IssuedAt = DateTime.Now
            };
        }
    }

    /// <summary>صياغة سطور الفلاتر في الـ Header بشكل موحّد (بتتكتب بس لو الفلتر مطبّق فعلاً).</summary>
    public static class ExportFilterText
    {
        private static readonly string[] ArabicMonths =
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        public static string FormatDate(DateTime value) =>
            value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public static string FormatDateTime(DateTime value) =>
            value.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);

        public static bool TryParseDate(string? text, out DateTime value) =>
            DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

        /// <summary>فلتر التاريخ بنفس أوضاع صفحات الطلبات: today | week | month | custom | all</summary>
        public static List<string> FromDateMode(string? mode, string? from, string? to)
        {
            var lines = new List<string>();
            var m = (mode ?? "all").Trim().ToLowerInvariant();

            var hasFrom = TryParseDate(from, out var f);
            var hasTo = TryParseDate(to, out var t);

            switch (m)
            {
                case "today":
                    if (hasFrom) lines.Add($"التاريخ: {FormatDate(f)}");
                    break;

                case "month":
                    if (hasFrom) lines.Add($"الشهر: {ArabicMonths[f.Month - 1]} {f.Year}");
                    break;

                case "week":
                    if (hasFrom && hasTo) lines.Add($"الفترة: من {FormatDate(f)} إلى {FormatDate(t)} (آخر 7 أيام)");
                    break;

                case "custom":
                    if (hasFrom && hasTo) lines.Add($"الفترة: من {FormatDate(f)} إلى {FormatDate(t)}");
                    else if (hasFrom) lines.Add($"الفترة: من {FormatDate(f)}");
                    else if (hasTo) lines.Add($"الفترة: حتى {FormatDate(t)}");
                    break;
            }

            return lines;
        }

        /// <summary>فلتر "من / إلى" (تاريخ ووقت) زي صفحة أوامر التوريد. بيرجع null لو مفيش فلتر.</summary>
        public static string? Period(DateTime? from, DateTime? to)
        {
            if (from.HasValue && to.HasValue) return $"الفترة: من {FormatDateTime(from.Value)} إلى {FormatDateTime(to.Value)}";
            if (from.HasValue) return $"الفترة: من {FormatDateTime(from.Value)}";
            if (to.HasValue) return $"الفترة: حتى {FormatDateTime(to.Value)}";
            return null;
        }

        public static string? Search(string? term) =>
            string.IsNullOrWhiteSpace(term) ? null : $"بحث: {term.Trim()}";
    }
}
