namespace Rassef.Models.Configurations
{
    public class RequestStatusesConfiguration : IEntityTypeConfiguration<RequestStatuses>
    {
        public void Configure(EntityTypeBuilder<RequestStatuses> builder)
        {
            builder.ToTable("RequestStatuses");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.TransferRequests)
                   .WithOne(x => x.RequestStatus)
                   .HasForeignKey(x => x.RequestStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SupplierRequests)
                   .WithOne(x => x.RequestStatus)
                   .HasForeignKey(x => x.RequestStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}