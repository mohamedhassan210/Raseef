namespace Rassef.Models.Identity
{
    public class UserWarehouse : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}