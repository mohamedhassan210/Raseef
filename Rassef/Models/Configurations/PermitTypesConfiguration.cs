namespace Rassef.Models.Configurations
{
    public class PermitTypesConfiguration : IEntityTypeConfiguration<PermitTypes>
    {
        public void Configure(EntityTypeBuilder<PermitTypes> builder)
        {
            builder.ToTable("PermitTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.TransferRequests)
                   .WithOne(x => x.PermitType)
                   .HasForeignKey(x => x.PermitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SupplierRequests)
                   .WithOne(x => x.PermitType)
                   .HasForeignKey(x => x.PermitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}