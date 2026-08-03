namespace Rassef.Models.Configurations
{
    public class DriverTypeConfiguration : IEntityTypeConfiguration<DriverType>
    {
        public void Configure(EntityTypeBuilder<DriverType> builder)
        {
            builder.ToTable("DriverTypes");

            builder.Property(x => x.Code);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.Drivers)
                   .WithOne(x => x.DeiverType)
                   .HasForeignKey(x => x.DeiverTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}