using System.Collections.Generic;

namespace Rassef.ViewModels.Group
{
    public class RolesManagementVM
    {
        public List<GroupCardVM> Groups { get; set; } = new();
        public List<PermissionTableItemVM> Permissions { get; set; } = new();
    }

    public class PermissionTableItemVM
    {
        public int Id { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
