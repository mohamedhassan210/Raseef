using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rassef.ViewModels.CheckOut
{
    public class UpdateCheckOutVM
    {
        [Required(ErrorMessage = "المعرف (ID) مطلوب.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "يرجى اختيار التذكرة.")]
        [Display(Name = "التذكرة")]
        public Guid TicketId { get; set; }

        public IEnumerable<SelectListItem>? Tickets { get; set; }

        [Required(ErrorMessage = "يرجى اختيار نوع الخروج.")]
        [Display(Name = "نوع الخروج")]
        public Guid ExitTypeId { get; set; }

        public IEnumerable<SelectListItem>? ExitTypes { get; set; }

        [Required(ErrorMessage = "يرجى إدخال وقت الخروج.")]
        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}