public class CreateDockAssignmentVM
{
    [Display(Name = "الرصيف")]
    public int DockId { get; set; }

    [Display(Name = "التذكرة")]
    public int TicketId { get; set; }

    [Display(Name = "وقت التعيين")]
    public DateTimeOffset AssignedAt { get; set; }

    [Display(Name = "وقت الانتهاء")]
    public DateTimeOffset FinishedAt { get; set; }

    public IEnumerable<SelectListItem> Docks { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Tickets { get; set; } = new List<SelectListItem>();
}