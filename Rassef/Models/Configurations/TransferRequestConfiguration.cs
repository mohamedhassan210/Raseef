namespace Rassef.Configurations
{
    public class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
    {
        public void Configure(EntityTypeBuilder<TransferRequest> builder)
        {
            builder.ToTable("TransferRequests");

            builder.Property(tr => tr.AvizNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(tr => tr.DriverPhone)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(tr => tr.PermitNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(tr => tr.Truck)
                   .WithMany(t => t.TransferRequests)
                   .HasForeignKey(tr => tr.TruckId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tr => tr.Driver)
                   .WithMany(d => d.TransferRequests)
                   .HasForeignKey(tr => tr.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tr => tr.Department)
                   .WithMany(d => d.TransferRequests)
                   .HasForeignKey(tr => tr.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tr => tr.PermitType)
                   .WithMany()
                   .HasForeignKey(tr => tr.PermitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tr => tr.RequestStatus)
                   .WithMany()
                   .HasForeignKey(tr => tr.RequestStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tr => tr.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}