using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rassef.ViewModels.Driver
{
    public class CreateDriverVM
    {
        [Display(Name = "اسم السائق")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "الرقم القومي")]
        public string NationalId { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "المورد")]
        public int? SupplierId { get; set; }


        [Display(Name = "اسم الشركة")]
        public string? SupplierName { get; set; }

        [Display(Name = "رقم الشاحنة")]
        public int? TruckId { get; set; }


        public IEnumerable<SelectListItem> Suppliers { get; set; }
            = new HashSet<SelectListItem>();
    }
}