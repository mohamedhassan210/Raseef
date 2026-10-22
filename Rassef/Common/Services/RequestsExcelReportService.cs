namespace Rassef.Common.Interfaces.Services.FileServices
{
    /// <summary>
    /// قديمة — اتحوّل بناء التقارير لـ ExcelReportBuilder (static، من غير DI) في
    /// ExcelReportService.cs. الـ class دي مبقتش بتتسجّل ولا بتتستخدم من أي حتة، وسايبينها
    /// بس عشان أي نسخة قديمة من DependencyInjection.cs لسه فيها سطر تسجيلها تفضل تتبني
    /// من غير أخطاء compile. ممكن تتمسح مع سطر التسجيل في أي وقت.
    /// </summary>
    public class RequestsExcelReportService
    {
        public byte[] Build(ExcelReportMeta meta, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
            => ExcelReportBuilder.BuildFromStrings(meta, headers, rows);
    }
}
