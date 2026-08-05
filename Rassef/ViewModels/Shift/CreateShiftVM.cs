public class CreateShiftVM
{
    [Required(ErrorMessage = "اسم الشيفت مطلوب")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "وقت بداية الشيفت مطلوب")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "مدة الشيفت مطلوبة")]
    public TimeSpan Duration { get; set; }
}