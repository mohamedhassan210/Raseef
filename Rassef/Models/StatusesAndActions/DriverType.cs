namespace Rassef.Models.StatusesAndActions
{
    public class DriverType : BaseEntity
    {
        public int? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Driver> Drivers { get; set; } = new HashSet<Driver>();
    }
}
