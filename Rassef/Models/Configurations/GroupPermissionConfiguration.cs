using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rassef.Models.Identity;

namespace Rassef.Configurations
{
    public class GroupPermissionConfiguration : IEntityTypeConfiguration<GroupPermission>
    {
        public void Configure(EntityTypeBuilder<GroupPermission> builder)
        {
            builder.ToTable("GroupPermissions");

            builder.HasKey(gp => new { gp.GroupId, gp.PermissionId });

            builder.HasOne(gp => gp.Group)
                   .WithMany()
                   .HasForeignKey(gp => gp.GroupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(gp => gp.Permission)
                   .WithMany()
                   .HasForeignKey(gp => gp.PermissionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}