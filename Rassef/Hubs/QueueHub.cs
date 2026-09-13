using Microsoft.AspNetCore.SignalR;

namespace Rassef.Hubs
{
    /// <summary>
    /// Hub بسيط بدون منطق داخلي: كل اللي بيعمله إنه قناة بث (Broadcast Channel).
    /// أي تغيير في حالة أي تذكرة (استدعاء دور جديد / تحديث حالة / تسجيل خروج)
    /// بيبعت منه إشعار "QueueUpdated" لكل الأجهزة المتصلة (شاشات العرض + صفحات
    /// متابعة الدور بتاعة السواقين)، وكل جهاز بيرد بعمل GetLiveStatus بتاعه
    /// عشان ياخد بياناته الشخصية المحدّثة.
    /// </summary>
    public class QueueHub : Hub
    {
    }
}
