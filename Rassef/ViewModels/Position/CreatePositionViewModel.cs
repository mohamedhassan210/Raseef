namespace Rassef.ViewModels.Position
{
    public class CreatePositionViewModel
    {

        [Display(Name = "كود المنصب")]
        public int PositionCode { get; set; }

        [Display(Name = "اسم المنصب")]
        public string PositionName { get; set; } = string.Empty;
    }
}