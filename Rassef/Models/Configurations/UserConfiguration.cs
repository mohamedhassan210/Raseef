namespace Rassef.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(u => u.UserName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.HashPassword)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.NationalId)
                   .IsRequired()
                   .HasMaxLength(14);

            builder.HasIndex(u => u.NationalId)
                   .IsUnique();

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .HasMaxLength(256)
                     .IsRequired();
            });

            builder.HasOne(u => u.Group)
                   .WithMany(g => g.Users)
                   .HasForeignKey(u => u.GroupId)
                   // ARCH-4: تغيير من Cascade إلى Restrict - حذف المجموعة لن يحذف كل موظفيها تلقائياً
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Position)
                   .WithMany(p => p.Users)
                   .HasForeignKey(u => u.PositionId)
                   // ARCH-4: تغيير من Cascade إلى Restrict - حذف المنصب لن يحذف كل موظفيه تلقائياً
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Suppliers)
                .WithOne(x => x.CreatedBy)
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}