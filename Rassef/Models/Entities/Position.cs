namespace Rassef.Models.Entities
{
    public class Position : BaseEntity
    {
        public int PositionCode { get; set; }
        public string PositionName { get; set; } = string.Empty;
       public ICollection<User> Users { get; set; } = new List<User>();
    }
}
