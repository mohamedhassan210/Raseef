using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rassef.Models.Configurations
{
    public class CheckOutConfiguration : IEntityTypeConfiguration<CheckOut>
    {
        public void Configure(EntityTypeBuilder<CheckOut> builder)
        {
            builder.ToTable("CheckOuts");

            builder.Property(c => c.ExitTime)
                   .IsRequired();

            builder.HasOne(c => c.QueueTicket)
                   .WithOne(q => q.CheckOut)
                   .HasForeignKey<CheckOut>(c => c.TicketId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ExitType)
                   .WithMany() 
                   .HasForeignKey(c => c.ExitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
