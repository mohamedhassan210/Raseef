namespace Rassef.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public User CreatedBy { get; set; }

        public ICollection<UserWarehouse> UserWarehouses { get; set; } = new HashSet<UserWarehouse>();
        public ICollection<Dock> Docks { get; set; } = new HashSet<Dock>();
        public ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    }
}