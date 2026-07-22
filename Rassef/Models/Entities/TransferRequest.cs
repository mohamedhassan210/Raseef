using Rassef.Models.Common;

namespace Rassef.Models.Entities
{
    public class TransferRequest : BaseEntity
    {
      public string AvizNumber { get; set; }
      public int TruckId { get; set; }
      public int DriverId { get; set; }
      public int PermitTypeId { get; set; }
      public int DepartmentId { get; set; } 
      public int RequestStatusId { get; set; } 
       public string DriverPhone { get; set; }
      public string PermitNumber { get; set; }
      public int CreatedBy { get; set; }


    }
}
