namespace Rassef.ViewModels.Truck
{
    public class UpdateTruckVM
    {
        [Required(ErrorMessage = "معرف الشاحنة مطلوب.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رقم اللوحة مطلوب.")]
        [StringLength(10, MinimumLength = 1,
            ErrorMessage = "يجب أن يكون رقم اللوحة بين 1 و10 أحرف.")]
        [RegularExpression(@"^[A-Za-z0-9]+$",
            ErrorMessage = "رقم اللوحة يجب أن يحتوي على حروف إنجليزية وأرقام فقط.")]
        [Display(Name = "رقم اللوحة")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "حروف اللوحة مطلوبة.")]
        [StringLength(5, MinimumLength = 1,
            ErrorMessage = "يجب أن تتكون حروف اللوحة من 1 إلى 5 أحرف.")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "حروف اللوحة يجب أن تحتوي على حروف إنجليزية فقط.")]
        [Display(Name = "حروف اللوحة")]
        public string PlateLetter { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعة التخزينية مطلوبة.")]
        [Range(0.1, 1000,
            ErrorMessage = "يجب أن تكون السعة التخزينية بين 0.1 و1000 طن.")]
        [Display(Name = "السعة التخزينية")]
        public double StorageCapacity { get; set; }

        [Display(Name = "شاحنة مبردة")]
        public bool IsRefrigerated { get; set; }

        [Required(ErrorMessage = "نوع الشاحنة مطلوب.")]
        // [NotEmptyGuid(ErrorMessage = "يرجى اختيار نوع الشاحنة.")]
        [Display(Name = "نوع الشاحنة")]
        public Guid TruckTypeId { get; set; }
    }
}