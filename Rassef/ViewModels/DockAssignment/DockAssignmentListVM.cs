public class DockAssignmentListVM
{
    public int Id { get; set; }

    [Display(Name = "اسم الرصيف")]
    public string DockName { get; set; } = string.Empty;

    [Display(Name = "رقم التذكرة")]
    public string TicketNumber { get; set; } = string.Empty;

    [Display(Name = "وقت التعيين")]
    public DateTimeOffset AssignedAt { get; set; }

    [Display(Name = "وقت الانتهاء")]
    public DateTimeOffset FinishedAt { get; set; }
}