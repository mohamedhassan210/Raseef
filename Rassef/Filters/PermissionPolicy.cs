namespace Rassef.Filters
{
    public static class PermissionPolicy
    {
        public static readonly HashSet<string> AdminOnlyControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Administration",
            "Group",
            "Shift",
            "QueueSettings"
        };
    }
}