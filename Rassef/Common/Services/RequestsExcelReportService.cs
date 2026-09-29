using System.Globalization;

namespace Rassef.Common.Interfaces.Services.FileServices
{
    /// <summary>
    /// بيانات الـ Header والـ Footer اللي بتظهر جوه ملف الـ Excel.
    /// </summary>
    public class RequestsReportMeta
    {
        /// <summary>عنوان التقرير، مثلاً: طلبات تحويل</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>اسم الورقة (Sheet) في الملف</summary>
        public string SheetName { get; set; } = "Report";

        /// <summary>اسم المخزن (اختياري)</summary>
        public string? WarehouseName { get; set; }

        /// <summary>سطور الفلاتر المطبّقة (شهر / فترة / قسم / بحث). تتكتب بس لو فيه فلتر فعلاً.</summary>
        public List<string> FilterLines { get; set; } = new();

        /// <summary>اسم الشخص اللي حرّر التقرير (المستخدم اللي عامل login)</summary>
        public string IssuedBy { get; set; } = string.Empty;

        /// <summary>تاريخ ووقت إصدار التقرير</summary>
        public DateTime IssuedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// بيبني ملف Excel لتقارير طلبات التوريد/التحويل:
    /// Header (العنوان + المخزن + الفلاتر) في أول الشيت، الجدول، وبعدها Footer
    /// (محرّر التقرير + تاريخ الإصدار). كمان بيضبط Header/Footer الخاصين بالطباعة.
    /// </summary>
    public class RequestsExcelReportService
    {
        private const string HeaderFill = "#E29547";
        private const string TitleFill = "#EB842D";
        private const string SoftFill = "#FDF1E7";

        public byte[] Build(RequestsReportMeta meta, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
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
                    ws.Cell(dataRow, c + 1).Value = c < row.Length ? (row[c] ?? string.Empty) : string.Empty;
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
            r = WriteMergedLine(ws, r, colCount, $"عدد الطلبات: {rows.Count}", 12, bold: true, fill: SoftFill);
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
}
