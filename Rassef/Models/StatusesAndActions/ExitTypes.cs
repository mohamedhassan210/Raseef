namespace Rassef.Models.StatusesAndActions
{
    public class ExitTypes : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<CheckOut> CheckOuts { get; set; } = new HashSet<CheckOut>();
    }
}
