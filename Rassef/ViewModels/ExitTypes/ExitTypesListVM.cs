namespace Rassef.ViewModels.ExitTypes
{
    public class ExitTypesListVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم نوع الخروج")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد عمليات المغادرة المرتبطة")]
        public int CheckOutsCount { get; set; }
    }
}
