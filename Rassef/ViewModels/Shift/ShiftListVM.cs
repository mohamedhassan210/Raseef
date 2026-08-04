namespace Rassef.ViewModels.Shift
{
    public class ShiftListVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public TimeSpan Duration { get; set; }
    }
}