namespace Rassef.ViewModels.DepartmentType
{
    public class UpdateDepartmentTypesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "اسم نوع القسم")]
        public string Name { get; set; } = string.Empty;
    }
}
