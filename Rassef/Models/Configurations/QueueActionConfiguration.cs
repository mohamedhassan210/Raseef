namespace Rassef.Configurations
{
    public class QueueActionConfiguration : IEntityTypeConfiguration<QueueAction>
    {
        public void Configure(EntityTypeBuilder<QueueAction> builder)
        {
            builder.ToTable("QueueActions");

            builder.Property(qa => qa.ActionTime)
                   .IsRequired();

            builder.HasOne(qa => qa.QueueTicket)
                   .WithMany(qt => qt.QueueActions)
                   .HasForeignKey(qa => qa.TicketId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qa => qa.ActionType)
                   .WithMany()
                   .HasForeignKey(qa => qa.ActionTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}