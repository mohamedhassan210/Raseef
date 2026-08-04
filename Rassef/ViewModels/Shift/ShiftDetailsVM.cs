public class ShiftDetailsVM
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime StartDate { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTimeOffset CreatedAT { get; set; }
}