namespace Rassef.Models.StatusesAndActions
{
    public class TruckTypes : BaseEntity
    {
        public string Name { get; protected set; } = string.Empty;
        public ICollection<Truck> Trucks  { get; set; } = new HashSet<Truck>();
    }
}
