namespace Rassef.ViewModels.Shift
{
    public class ShiftDetailsVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTimeOffset CreatedAT { get; set; }
    }
}