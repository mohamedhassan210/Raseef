namespace Rassef.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public User CreatedBy { get; set; }

    }
}
