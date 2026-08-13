namespace Rassef.ViewModels.DepartmentType
{
    public class DepartmentTypesDetailsVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم نوع القسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "عدد الأقسام المرتبطة")]
        public int DepartmentsCount { get; set; }
    }
}
