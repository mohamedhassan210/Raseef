using System.ComponentModel.DataAnnotations;

namespace Rassef.ViewModels.Shift
{
    public class CreateShiftVM
    {
        [Required(ErrorMessage = "اسم الشيفت مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "مدة الشيفت مطلوبة")]
        public TimeSpan Duration { get; set; }
    }
}