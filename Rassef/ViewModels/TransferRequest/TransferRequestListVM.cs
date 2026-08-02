public class TransferRequestListVM
{
    public int Id { get; set; }

    public string AvizNumber { get; set; } = string.Empty;

    public bool IsRefrigerated { get; set; }
    public bool IsFood{ get; set; }
    public string Truck { get; set; } = string.Empty;

    public string Driver { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string RequestStatus { get; set; } = string.Empty;
}