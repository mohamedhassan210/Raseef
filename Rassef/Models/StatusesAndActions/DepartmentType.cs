namespace Rassef.Models.StatusesAndActions
{
    public class DepartmentType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    }
}
