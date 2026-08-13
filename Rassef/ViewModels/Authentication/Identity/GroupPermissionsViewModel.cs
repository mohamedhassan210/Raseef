namespace Rassef.ViewModels.Authentication.Identity
{
    public class GroupPermissionsViewModel
    {
        public int GroupId { get; set; }

        [Display(Name = "اسم المجموعة")]
        public string GroupName { get; set; } = string.Empty;

        public List<ControllerPermissionsViewModel> Controllers { get; set; } = new();
    }
}

