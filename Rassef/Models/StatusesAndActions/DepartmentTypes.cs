namespace Rassef.Models.StatusesAndActions
{
    public class DepartmentTypes : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    }
}
