namespace Rassef.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(s => s.Phone)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(s => s.LogoURL)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.HasOne(s => s.CreatedBy)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany(x => x.Suppliers)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}