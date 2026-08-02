

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

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .HasMaxLength(256)
                     .IsRequired();
            });

            builder.HasOne(u => u.Group)
                   .WithMany(g => g.Users)
                   .OnDelete(DeleteBehavior.Cascade);

            // Position Relation 
            builder.HasOne(u => u.Position)
                .WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}