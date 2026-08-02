namespace Rassef.Persistence.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("Positions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PositionCode)
                .IsRequired();

            builder.Property(p => p.PositionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => p.PositionCode)
                .IsUnique();

            builder.HasIndex(p => p.PositionName)
                .IsUnique();

            builder.HasMany(p => p.Users)
                .WithOne(u => u.Position)
                .HasForeignKey(u => u.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}