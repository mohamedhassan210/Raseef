using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rassef.Models.Identity;

namespace Rassef.Configurations
{
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroups");

            builder.Property(ug => ug.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasMany(ug => ug.GroupPermissions)
                   .WithOne(gp => gp.Group)
                   .HasForeignKey(gp => gp.GroupId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}