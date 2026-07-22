using Rassef.Models.Common;

namespace Rassef.Models.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public int WarehouseId { get; set; }
        public int CreatedBy { get; set; }

    }
}
