namespace Rassef.Common.Interfaces.Services.FileServices
{
    public class ExcelExportService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Report")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.RightToLeft = true;

            PropertyInfo[] props = typeof(T).GetProperties();

            // عناوين الأعمدة
            for (int i = 0; i < props.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = props[i].Name.Replace("_", " ");
            }

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#E29547");
            headerRow.Style.Font.FontColor = XLColor.White;

            // البيانات
            int rowIndex = 2;
            foreach (var item in data)
            {
                for (int colIndex = 0; colIndex < props.Length; colIndex++)
                {
                    var val = props[colIndex].GetValue(item, null);
                    worksheet.Cell(rowIndex, colIndex + 1).Value = val?.ToString() ?? "";
                }
                rowIndex++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}