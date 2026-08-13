namespace Rassef.Models.Configurations
{
    public class TicketStatusesConfiguration : IEntityTypeConfiguration<TicketStatuses>
    {
        public void Configure(EntityTypeBuilder<TicketStatuses> builder)
        {
            builder.ToTable("TicketStatuses");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.QueueTickets)
                   .WithOne(x => x.TicketStatus)
                   .HasForeignKey(x => x.TicketStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}