namespace Rassef.ViewModels.Driver
{
    public class UpdateDriverVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public int? SupplierId { get; set; }
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
    }
}