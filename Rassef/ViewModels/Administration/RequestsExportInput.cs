namespace Rassef.ViewModels.Administration
{
    /// <summary>
    /// ما يبعته الـ Browser عند الضغط على "تصدير" في صفحتي طلبات التوريد/التحويل:
    /// الطلبات الظاهرة فعلاً على الشاشة (بعد كل الفلاتر بنفس ترتيبها) + وصف الفلاتر
    /// المطبّقة، عشان الملف المُصدَّر يطابق اللي المستخدم شايفه بالظبط.
    /// </summary>
    public class RequestsExportInput
    {
        /// <summary>أرقام الطلبات الظاهرة على الشاشة بنفس ترتيب ظهورها</summary>
        public List<int> Ids { get; set; } = new();

        /// <summary>today | week | month | custom | all</summary>
        public string? Mode { get; set; }

        /// <summary>yyyy-MM-dd — بداية الفترة (لو فيه فلتر تاريخ)</summary>
        public string? From { get; set; }

        /// <summary>yyyy-MM-dd — نهاية الفترة (لو فيه فلتر تاريخ)</summary>
        public string? To { get; set; }

        /// <summary>اسم القسم المختار في فلتر القسم (فاضي = كل الأقسام)</summary>
        public string? Department { get; set; }

        /// <summary>نص البحث في مربع البحث</summary>
        public string? Search { get; set; }
    }
}
