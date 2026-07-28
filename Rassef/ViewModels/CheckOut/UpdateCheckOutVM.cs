using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rassef.ViewModels.CheckOut
{
    public class UpdateCheckOutVM
    {
        public Guid Id { get; set; }

        [Display(Name = "التذكرة")]
        public Guid TicketId { get; set; }

        public IEnumerable<SelectListItem>? Tickets { get; set; }

        [Display(Name = "نوع الخروج")]
        public Guid ExitTypeId { get; set; }

        public IEnumerable<SelectListItem>? ExitTypes { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTimeOffset ExitTime { get; set; }
    }
}