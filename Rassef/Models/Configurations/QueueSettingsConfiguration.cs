namespace Rassef.Models.Configurations
{
    public class QueueSettingsConfiguration : IEntityTypeConfiguration<QueueSettings>
    {
        public void Configure(EntityTypeBuilder<QueueSettings> builder)
        {
            builder.ToTable("QueueSettings");

            builder.Property(x => x.ResetType)
                   .IsRequired();

            builder.HasOne(x => x.Shift)
                   .WithMany(x => x.QueueSettings)
                   .HasForeignKey(x => x.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}