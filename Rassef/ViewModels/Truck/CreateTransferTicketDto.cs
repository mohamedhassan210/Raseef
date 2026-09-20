namespace Rassef.ViewModels.Truck
{
    public class CreateTransferTicketDto
    {
        public int TruckId { get; set; }
        public int DepartmentId { get; set; }
        public int? DriverId { get; set; }
        public int? DockId { get; set; }
    }
}
