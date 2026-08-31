using Microsoft.EntityFrameworkCore;
using Rassef.Data;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.ValueObjects;
using Rassef.Common.Helpers;

namespace Rassef.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();

            try
            {
                // 1. Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // 2. Ensure Admin Group exists
                var adminGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "Admin");
                if (adminGroup == null)
                {
                    adminGroup = new UserGroup
                    {
                        Name = "Admin"
                    };
                    context.UserGroups.Add(adminGroup);
                    await context.SaveChangesAsync();
                }

                // 3. Ensure Admin Position exists
                var adminPosition = await context.Positions.FirstOrDefaultAsync(p => p.PositionName == "Admin");
                if (adminPosition == null)
                {
                    adminPosition = new Position
                    {
                        PositionName = "Admin"
                    };
                    context.Positions.Add(adminPosition);
                    await context.SaveChangesAsync();
                }

                // 4. Ensure Permissions are discovered & seeded
                var discoveredPermissions = PermissionHelper.DiscoverPermissions();
                var existingPermissions = await context.Permissions.ToListAsync();
                var existingKeys = existingPermissions
                    .Select(p => $"{p.ControllerName}_{p.ActionName}".ToLowerInvariant())
                    .ToHashSet();

                var newPermissions = new List<Permission>();
                foreach (var perm in discoveredPermissions)
                {
                    var key = $"{perm.ControllerName}_{perm.ActionName}".ToLowerInvariant();
                    if (!existingKeys.Contains(key))
                    {
                        var entity = new Permission
                        {
                            ControllerName = perm.ControllerName,
                            ActionName = perm.ActionName,
                            Description = perm.Description
                        };
                        newPermissions.Add(entity);
                        context.Permissions.Add(entity);
                    }
                }

                if (newPermissions.Any())
                {
                    await context.SaveChangesAsync();
                }

                // 5. Ensure all permissions are granted to Admin group
                var allPermissions = await context.Permissions.ToListAsync();
                var existingGroupPermissions = (await context.GroupPermissions
                    .Where(gp => gp.GroupId == adminGroup.Id)
                    .Select(gp => gp.PermissionId)
                    .ToListAsync())
                    .ToHashSet();

                foreach (var perm in allPermissions)
                {
                    if (!existingGroupPermissions.Contains(perm.Id))
                    {
                        context.GroupPermissions.Add(new GroupPermission
                        {
                            GroupId = adminGroup.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
                await context.SaveChangesAsync();

                // 6. Ensure Admin User exists
                var adminUser = await context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UserName == "admin" || (u.Email != null && u.Email.Value == "admin@rassef.com"));

                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        Name = "مدير النظام",
                        UserName = "admin",
                        Email = Email.Create("admin@rassef.com"),
                        Phone = "01000000000",
                        NationalId = "12345678901234",
                        UserCode = "ADMIN-001",
                        BranchCode = "HQ",
                        PositionId = adminPosition.Id,
                        GroupId = adminGroup.Id,
                        IsChanged = true, // Set to true so password doesn't have to be forced-reset on first login
                        HashPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123456")
                    };
                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // Ensure position, group and active state for admin
                    bool updated = false;
                    if (adminUser.GroupId != adminGroup.Id)
                    {
                        adminUser.GroupId = adminGroup.Id;
                        updated = true;
                    }
                    if (adminUser.PositionId != adminPosition.Id)
                    {
                        adminUser.PositionId = adminPosition.Id;
                        updated = true;
                    }
                    if (adminUser.IsDeleted)
                    {
                        adminUser.IsDeleted = false;
                        updated = true;
                    }
                    if (updated)
                    {
                        adminUser.MarkAsUpdated();
                        await context.SaveChangesAsync();
                    }
                }

                // 7. Ensure Departments have their Prefix in DB
                var departments = await context.Departments.ToListAsync();
                bool deptsUpdated = false;
                char defaultLetter = 'A';
                foreach (var dept in departments)
                {
                    if (string.IsNullOrWhiteSpace(dept.Prefix))
                    {
                        dept.Prefix = defaultLetter.ToString();
                        dept.MarkAsUpdated();
                        deptsUpdated = true;
                    }
                    defaultLetter = (char)(defaultLetter + 1);
                }
                if (deptsUpdated)
                {
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during database seeding for Admin user.");
            }
        }
    }
}
