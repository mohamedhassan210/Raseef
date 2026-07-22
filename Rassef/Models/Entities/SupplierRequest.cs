namespace Rassef.Models.Entities
{
    public class SupplierRequest : BaseEntity
    {
        public int SupplierId { get; set; }
        public int TruckId { get; set; }
        public int DriverId { get; set; }
        public int DepartmentId { get; set; }
        public int PermitTypeId { get; set; }
        public int CommodityTypeId { get; set; }
        public int RequestStatusId { get; set; }
        public string DriverNationalCardPhoto { get; set; }
        public string DriverPhone { get; set; }
        public string PermitNumber { get; set; }
        public int CreatedBy { get; set; }

    }
}
