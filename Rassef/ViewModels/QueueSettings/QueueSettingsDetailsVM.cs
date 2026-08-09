namespace Rassef.ViewModels.QueueSettings
{
    public class QueueSettingsDetailsVM
    {
        public int Id { get; set; }

        public string ResetType { get; set; } = string.Empty;

        public string? ShiftName { get; set; }

        public DateTimeOffset CreatedAT { get; set; }
    }
}