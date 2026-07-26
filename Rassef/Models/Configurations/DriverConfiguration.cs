namespace Rassef.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");

            builder.Property(d => d.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(d => d.NationalId)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(d => d.Phone)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(d => d.NationalCardPhoto)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.HasOne(d => d.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}