namespace Rassef.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.Property(p => p.ControllerName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.ActionName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Description)
                   .HasMaxLength(250);

            builder.HasIndex(p => new { p.ControllerName, p.ActionName })
                   .IsUnique();

            builder.HasMany(p => p.GroupPermissions)
                   .WithOne(gp => gp.Permission)
                   .HasForeignKey(gp => gp.PermissionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}