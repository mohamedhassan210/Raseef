namespace Rassef.Configurations
{
    public class DockAssignmentConfiguration : IEntityTypeConfiguration<DockAssignment>
    {
        public void Configure(EntityTypeBuilder<DockAssignment> builder)
        {
            builder.ToTable("DockAssignments");

            builder.Property(da => da.AssignedAt)
                   .IsRequired();

            builder.Property(da => da.FinishedAt)
                   .IsRequired();

            builder.HasOne(da => da.Dock)
                   .WithMany(d => d.DockAssignments)
                   .HasForeignKey(da => da.DockId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(da => da.QueueTicket)
                   .WithMany(qt => qt.DockAssignments)
                   .HasForeignKey(da => da.TicketId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(da => da.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}