namespace Rassef.ViewModels.QueueSettings
{
    public class CreateQueueSettingsVM
    {
        public ResetType ResetType { get; set; }

        public int? ShiftId { get; set; }

        public IEnumerable<SelectListItem> Shifts { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}