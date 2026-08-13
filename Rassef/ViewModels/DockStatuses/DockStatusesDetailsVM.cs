namespace Rassef.ViewModels.DockStatuses
{
    public class DockStatusesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم حالة الرصيف")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد الأرصفة المرتبطة")]
        public int DocksCount { get; set; }
    }
}
