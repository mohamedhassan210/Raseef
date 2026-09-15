namespace Rassef.Filters
{
    public static class PermissionPolicy
    {
        /// <summary>
        /// Controllers whose actions are decorated with the bare [PermissionAuthorize]
        /// (no controller/action passed to the attribute) but must still be gated by
        /// real, per-action GroupPermission rows instead of being treated as
        /// "any authenticated user is fine" (which is the bare attribute's normal
        /// meaning — see PermissionAuthorizeFilter).
        ///
        /// IMPORTANT: this list no longer causes an automatic deny for non-admins.
        /// It only tells the filter/service "for this controller, resolve the actual
        /// action name from the route and check the group's GroupPermissions — don't
        /// skip the check just because the attribute itself carried no arguments."
        /// A group that has been explicitly granted the permission (via
        /// Group/ManagePermissions) is allowed in; a group that hasn't is denied.
        /// There is no hardcoded "admins only" bypass here — Admins still pass via
        /// the isAdmin check earlier in the filter, same as every other controller.
        /// </summary>
        public static readonly HashSet<string> ExplicitPermissionControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Administration",
            "Group",
            "Shift",
            "QueueSettings"
        };
    }
}