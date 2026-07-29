namespace Rassef.ViewModels.Authentication.Identity
{
    public class PermissionCheckBoxViewModel
    {
        public int PermissionId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
