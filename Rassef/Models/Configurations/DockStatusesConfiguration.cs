namespace Rassef.Models.Configurations
{
    public class DockStatusesConfiguration : IEntityTypeConfiguration<DockStatuses>
    {
        public void Configure(EntityTypeBuilder<DockStatuses> builder)
        {
            builder.ToTable("DockStatuses");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.Docks)
                   .WithOne(x => x.DockStatus)
                   .HasForeignKey(x => x.DockStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}