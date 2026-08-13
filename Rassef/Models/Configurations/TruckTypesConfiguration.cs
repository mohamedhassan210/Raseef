namespace Rassef.Models.Configurations
{
    public class TruckTypesConfiguration : IEntityTypeConfiguration<TruckTypes>
    {
        public void Configure(EntityTypeBuilder<TruckTypes> builder)
        {
            builder.ToTable("TruckTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.Trucks)
                   .WithOne(x => x.TruckType)
                   .HasForeignKey(x => x.TruckTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}