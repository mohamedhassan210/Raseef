namespace Rassef.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.Property(w => w.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(w => w.Location)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.HasOne(w => w.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}