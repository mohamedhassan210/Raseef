namespace Rassef.ViewModels.QueueSettings
{
    public class QueueSettingsListVM
    {
        public int Id { get; set; }

        public string ResetType { get; set; } = string.Empty;

        public string? ShiftName { get; set; }
    }
}