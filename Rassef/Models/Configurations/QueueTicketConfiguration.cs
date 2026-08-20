namespace Rassef.Configurations
{
    public class QueueTicketConfiguration : IEntityTypeConfiguration<QueueTicket>
    {
        public void Configure(EntityTypeBuilder<QueueTicket> builder)
        {
            builder.ToTable("QueueTickets");

            builder.Property(qt => qt.TicketNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(qt => qt.QueueTime)
                   .IsRequired();

            builder.Property(qt => qt.EntryTime)
                   .IsRequired();

            builder.Property(qt => qt.ExitTime)
                   .IsRequired();

            // High-Performance Database Indexes for Ultra-Fast Lookups and Queue Sorting
            builder.HasIndex(qt => qt.TicketNumber);
            builder.HasIndex(qt => new { qt.TicketStatusId, qt.DepartmentId, qt.QueueTime });
            builder.HasIndex(qt => qt.ShiftId);
            builder.HasIndex(qt => qt.CreatedAT);

            builder.HasOne(qt => qt.TransferRequest)
                   .WithMany(tr => tr.QueueTickets)
                   .HasForeignKey(qt => qt.TransferRequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qt => qt.SupplierRequest)
                   .WithMany(sr => sr.QueueTickets)
                   .HasForeignKey(qt => qt.SupplierRequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qt => qt.Department)
                   .WithMany(d => d.QueueTickets)
                   .HasForeignKey(qt => qt.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qt => qt.TicketStatus)
                   .WithMany()
                   .HasForeignKey(qt => qt.TicketStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qt => qt.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}