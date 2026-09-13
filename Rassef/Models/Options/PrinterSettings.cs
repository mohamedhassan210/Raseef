namespace Rassef.Models.Options
{
    /// <summary>
    /// إعدادات مقاس ورق طابعة الإيصال — بتتقرأ من appsettings.json (قسم PrinterSettings).
    /// غيّرها من هناك وقت ما حبيت، من غير أي Migration أو تعديل في الكود.
    /// </summary>
    public class PrinterSettings
    {
        /// <summary>
        /// عرض الورقة بالمليمتر (مثال: 58 أو 80 لرول حراري، أو 100 لورقة/لابل مربع).
        /// </summary>
        public int ReceiptPaperWidthMm { get; set; } = 80;

        /// <summary>
        /// طول الورقة بالمليمتر. اسيبها فاضية (null) لو الطابعة رول حراري
        /// بيتقطع تلقائياً حسب طول المحتوى (Continuous Roll).
        /// حدد رقم بس لو الورقة/اللابل مقاس ثابت (زي 100×100).
        /// </summary>
        public int? ReceiptPaperHeightMm { get; set; } = null;
    }
}
