namespace Rassef.Models.Configurations
{
    public class CommodityTypesConfiguration : IEntityTypeConfiguration<CommodityTypes>
    {
        public void Configure(EntityTypeBuilder<CommodityTypes> builder)
        {
            builder.ToTable("CommodityTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.SupplierRequests)
                   .WithOne(x => x.CommodityType)
                   .HasForeignKey(x => x.CommodityTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}