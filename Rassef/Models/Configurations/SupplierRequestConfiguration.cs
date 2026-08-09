namespace Rassef.Configurations
{
    public class SupplierRequestConfiguration : IEntityTypeConfiguration<SupplierRequest>
    {
        public void Configure(EntityTypeBuilder<SupplierRequest> builder)
        {
            builder.ToTable("SupplierRequests");


            builder.Property(sr => sr.DriverNationalCardPhoto)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(sr => sr.DriverPhone)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(sr => sr.PermitNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(sr => sr.Supplier)
                   .WithMany(s => s.SupplierRequests)
                   .HasForeignKey(sr => sr.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Truck)
                   .WithMany(t => t.SupplierRequests)
                   .HasForeignKey(sr => sr.TruckId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Driver)
                   .WithMany(d => d.SupplierRequests)
                   .HasForeignKey(sr => sr.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Department)
                   .WithMany(d => d.SupplierRequests)
                   .HasForeignKey(sr => sr.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.PermitType)
                   .WithMany()
                   .HasForeignKey(sr => sr.PermitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.CommodityType)
                   .WithMany()
                   .HasForeignKey(sr => sr.CommodityTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.RequestStatus)
                   .WithMany()
                   .HasForeignKey(sr => sr.RequestStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}