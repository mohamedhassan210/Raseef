namespace Rassef.ViewModels.Truck
{
    public class CreateTransferTicketDto
    {
        public int TruckId { get; set; }
        public int DepartmentId { get; set; }
        public int? DriverId { get; set; }
        public int? DockId { get; set; }

        // NEW — بيانات الإذن بقت بتتدخل من المستخدم في مودال اختيار القسم بدل ما تتولّد تلقائي
        public string? AvizNumber { get; set; }
        public string? PermitNumber { get; set; }
        public int? PermitTypeId { get; set; }
    }
}
