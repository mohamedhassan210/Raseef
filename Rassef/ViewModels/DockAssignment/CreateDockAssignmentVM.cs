public class CreateDockAssignmentVM
{
    [Required(ErrorMessage = "برجاء اختيار الرصيف")]
    [Display(Name = "الرصيف")]
    public int DockId { get; set; }

    [Required(ErrorMessage = "برجاء اختيار التذكرة")]
    [Display(Name = "التذكرة")]
    public int TicketId { get; set; }

    [Required(ErrorMessage = "برجاء إدخال وقت التعيين")]
    [Display(Name = "وقت التعيين")]
    public DateTimeOffset AssignedAt { get; set; }

    [Required(ErrorMessage = "برجاء إدخال وقت الانتهاء")]
    [Display(Name = "وقت الانتهاء")]
    public DateTimeOffset FinishedAt { get; set; }

    public IEnumerable<SelectListItem> Docks { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Tickets { get; set; } = new List<SelectListItem>();
}