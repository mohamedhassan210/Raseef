using System.ComponentModel.DataAnnotations;

namespace Rassef.ViewModels.Truck
{
    public class UpdateTruckVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "برجاء إدخال رقم الشاحنة")]
        [Display(Name = "رقم الشاحنة")]
        public string TruckNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "برجاء إدخال حروف الشاحنة")]
        [Display(Name = "حروف الشاحنة")]
        public string TruckLetters { get; set; } = string.Empty;

        [Required(ErrorMessage = "برجاء إدخال سعة التخزين")]
        [Display(Name = "سعة التخزين")]
        public double Capacity { get; set; }

        [Display(Name = "نوع الشاحنة")]
        public bool IsRefrigerated { get; set; }

        [Display(Name = "شركة الشاحنة")]
        public string CompanyName { get; set; } = string.Empty;

        public int? SupplierId { get; set; }
    }
}