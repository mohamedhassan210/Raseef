namespace Rassef.ViewModels.Authentication.Identity
{
    public class ControllerPermissionsViewModel
    {
        public string ControllerName { get; set; } = string.Empty;
        public List<PermissionCheckBoxViewModel> Actions { get; set; } = new();
    }
}
