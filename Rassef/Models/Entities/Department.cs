
namespace Rassef.Models.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public int WarehouseId { get; set; }
        public User CreatedBy { get; set; }

    }
}
