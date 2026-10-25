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

        /// <summary>اسم العرض الفعلي (المخصّص لو اتعدّل، وإلا الاسم العربي الافتراضي)</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>true لو الاسم اتعدّل يدوياً (عشان نظهر شارة "مخصّص" وزرار "استعادة الاسم الافتراضي")</summary>
        public bool IsCustom { get; set; }
    }
}
