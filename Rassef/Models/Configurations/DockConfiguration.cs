namespace Rassef.Configurations
{
    public class DockConfiguration : IEntityTypeConfiguration<Dock>
    {
        public void Configure(EntityTypeBuilder<Dock> builder)
        {
            builder.ToTable("Docks");

            builder.Property(d => d.DockName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(d => d.Department)
                   .WithMany(dep => dep.Docks)
                   .HasForeignKey(d => d.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.DockStatus)
                   .WithMany()
                   .HasForeignKey(d => d.DockStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}