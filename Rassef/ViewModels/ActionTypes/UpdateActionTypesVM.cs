namespace Rassef.ViewModels.ActionTypes
{
    public class UpdateActionTypesVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "اسم الإجراء")]
        public string Name { get; set; } = string.Empty;
    }
}
