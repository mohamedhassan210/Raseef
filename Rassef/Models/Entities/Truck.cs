namespace Rassef.Models.Entities
{
    public class Truck : BaseEntity
    {
        public string PlateNumber { get; set; }
        public int TruckTypeId { get; set; }
        public float StorageCapacity { get; set; }
        public bool IsRefrigerated { get; set; }
        public bool IsFood { get; set; }
        public int CreatedBy { get; set; }

    }
}
