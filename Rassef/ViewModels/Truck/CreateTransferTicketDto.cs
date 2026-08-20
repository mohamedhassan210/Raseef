namespace Rassef.ViewModels.Truck
{
    public class CreateTransferTicketDto
    {
        public int TruckId { get; set; }
        public int DepartmentId { get; set; }
        public int? DriverId { get; set; }
    }
}
