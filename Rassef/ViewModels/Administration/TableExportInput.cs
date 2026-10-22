using System.Text.Json;

namespace Rassef.ViewModels.Administration
{
    /// <summary>
    /// تصدير جدول عام: الصفحة بتبعت عناوين الأعمدة والصفوف (اللي ظاهرة بعد الفلاتر) ووصف
    /// الفلاتر، والسيرفر بيرجّع ملف Excel بنفس الـ Header/Footer الموحّد.
    /// </summary>
    public class TableExportInput
    {
        /// <summary>عنوان التقرير في أول الملف، مثلاً: الموظفين</summary>
        public string? Title { get; set; }

        /// <summary>اسم الورقة (اختياري، الافتراضي = العنوان)</summary>
        public string? SheetName { get; set; }

        /// <summary>اسم الملف بدون امتداد (اختياري)</summary>
        public string? FileName { get; set; }

        /// <summary>اكتب اسم المخزن الحالي في الـ Header (افتراضي: نعم)</summary>
        public bool IncludeWarehouse { get; set; } = true;

        /// <summary>سطور الفلاتر المطبّقة (بحث / قسم / ...). فاضية = مفيش فلتر.</summary>
        public List<string>? FilterLines { get; set; }

        public List<string> Headers { get; set; } = new();

        /// <summary>كل صف = قيم بنفس ترتيب Headers (أرقام / نصوص / null)</summary>
        public List<List<JsonElement>> Rows { get; set; } = new();
    }
}
