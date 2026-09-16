namespace Rassef.ViewModels.Dock
{
    /// <summary>
    /// Feature — dock capacity + maintenance. One row per dock, carrying
    /// everything the Supplier/Transfer Request "pick a dock" field (and the
    /// Dock admin Index/Details pages) need to render availability without a
    /// second round trip: which department it belongs to, whether it's under
    /// maintenance, and its live occupancy vs capacity.
    /// </summary>
    public class DockOptionVM
    {
        public int Id { get; set; }
        public string DockName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public bool IsUnderMaintenance { get; set; }
        public int Occupancy { get; set; }
        public int MaxTruckCount { get; set; }

        public bool IsFull => Occupancy >= MaxTruckCount;

        /// <summary>
        /// Arabic suffix appended to the dock's name in the dropdown when it
        /// can't be picked — empty when the dock is selectable.
        /// </summary>
        public string DisabledReasonLabel
        {
            get
            {
                if (IsUnderMaintenance && IsFull) return " (في صيانة و ممتلئة)";
                if (IsUnderMaintenance) return " (في صيانة)";
                if (IsFull) return " (ممتلئة)";
                return string.Empty;
            }
        }

        public bool IsSelectable => !IsUnderMaintenance && !IsFull;
    }
}