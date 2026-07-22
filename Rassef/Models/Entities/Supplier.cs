namespace Rassef.Models.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public User CreatedBy { get; set; }

    }
}
