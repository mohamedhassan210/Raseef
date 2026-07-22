namespace Rassef.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public User CreatedBy { get; set; }

        public ICollection<Dock> Docks { get; set; }
        public ICollection<Department> Departments { get; set; }
    } 
}