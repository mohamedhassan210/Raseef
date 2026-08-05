

namespace Rassef.Models.Configurations
{
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
        public void Configure(EntityTypeBuilder<Shift> builder)
        {
            builder.ToTable("Shifts");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.StartTime)
          .IsRequired();

            builder.Property(x => x.Duration)
                   .IsRequired();

            builder.HasMany(x => x.QueueTickets)
                   .WithOne(x => x.Shift)
                   .HasForeignKey(x => x.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.QueueSettings)
                   .WithOne(x => x.Shift)
                   .HasForeignKey(x => x.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}