

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

            builder.HasMany(u => u.Groups)
                   .WithMany(g => g.Users)
                   .UsingEntity(j => j.ToTable("UserGroupMembers"));
        }
    }
}