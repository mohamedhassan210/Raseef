using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rassef.Models.Identity;

namespace Rassef.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasMany(p => p.GroupPermissions)
                   .WithOne(gp => gp.Permission)
                   .HasForeignKey(gp => gp.PermissionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}