namespace Rassef.Configurations
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder.ToTable("Trucks");

            builder.Property(t => t.PlateNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.PlateLetter)
                   .IsRequired()
                   .HasMaxLength(50);

            // Composite unique index on PlateNumber + PlateLetter
            builder.HasIndex(t => new { t.PlateNumber, t.PlateLetter })
                   .IsUnique();

            builder.Property(t => t.StorageCapacity)
                   .IsRequired();

            builder.Property(t => t.IsRefrigerated)
                   .IsRequired();


            builder.HasOne(t => t.TruckType)
                   .WithMany()
                   .HasForeignKey(t => t.TruckTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}