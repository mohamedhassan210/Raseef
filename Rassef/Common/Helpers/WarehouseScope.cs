namespace Rassef.Common.Helpers
{
    /// <summary>
    /// Feature 3 — single source of truth for "which warehouse is the user currently
    /// working in". The active warehouse is written to the SelectedWarehouseId cookie
    /// by AuthenticationController.SelectWarehouse (which validates that the user is
    /// actually assigned to it) and read back here.
    ///
    /// SCOPING CONVENTION used across the app: a null result means "unresolved" — the
    /// user has not picked a warehouse yet, or the cookie was cleared. Callers treat
    /// null as "do not filter" rather than "show nothing", so a user who has not picked
    /// a warehouse sees everything instead of an unexplained empty screen. That matches
    /// the behaviour DepartmentController/DockController already had before this helper
    /// existed.
    ///
    /// NOTE: this is a display-scoping helper. It filters what a user is offered; it
    /// does not by itself reject a tampered POST that names an out-of-scope id.
    /// </summary>
    public static class WarehouseScope
    {
        public const string CookieName = "SelectedWarehouseId";

        public static int? GetSelectedWarehouseId(this HttpRequest request)
        {
            if (request.Cookies.TryGetValue(CookieName, out var cookieValue)
                && int.TryParse(cookieValue, out var warehouseId))
            {
                return warehouseId;
            }

            return null;
        }
    }
}
