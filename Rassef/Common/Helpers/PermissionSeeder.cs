namespace Rassef.Common.Helpers
{
    public static class PermissionSeeder
    {
        public static async Task SyncPermissionsAsync(ApplicationDbContext context)
        {
            // Discover permissions from controllers
            var discoveredPermissions = PermissionHelper.DiscoverPermissions();

            // Existing permissions from database
            var existingPermissions = await context.Permissions
                .Select(p => new
                {
                    p.ControllerName,
                    p.ActionName
                })
                .ToListAsync();

            // Fast lookup
            var existingKeys = existingPermissions
                .Select(p => $"{p.ControllerName}:{p.ActionName}")
                .ToHashSet();

            // Missing permissions
            var newPermissions = discoveredPermissions
                .Where(p =>
                    !existingKeys.Contains(
                        $"{p.ControllerName}:{p.ActionName}"))
                .Select(p => new Permission
                {
                    ControllerName = p.ControllerName,
                    ActionName = p.ActionName,
                    Description = p.Description
                })
                .ToList();

            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}