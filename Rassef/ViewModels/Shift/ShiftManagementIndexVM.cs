namespace Rassef.ViewModels.Shift
{
    public class ShiftManagementIndexVM
    {
        // ── Queue Settings ──
        public ResetType CurrentResetType { get; set; } = ResetType.Daily;
        public int? CurrentActiveShiftId { get; set; }
        public DateTimeOffset? LastGlobalResetAt { get; set; }

        // ── Shifts List ──
        public List<ShiftItemDetailVM> Shifts { get; set; } = new();

        // ── Departments List (for Department Reset & Prefix overview) ──
        public List<DepartmentResetVM> Departments { get; set; } = new();
    }

    public class ShiftItemDetailVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset? LastResetAt { get; set; }
        public bool IsActiveNow { get; set; }
    }

    public class DepartmentResetVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public DateTimeOffset? LastResetAt { get; set; }
    }
}
