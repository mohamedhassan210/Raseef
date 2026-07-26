namespace Rassef.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasOne(d => d.Warehouse)
                   .WithMany(w => w.Departments)
                   .HasForeignKey(d => d.WarehouseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}