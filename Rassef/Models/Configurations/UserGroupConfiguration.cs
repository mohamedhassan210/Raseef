namespace Rassef.Models.Configurations
{
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            // 1. اسم الجدول
            builder.ToTable("UserGroups");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(150);


            builder.HasMany(ug => ug.Users)
                   .WithOne(u => u.Group)
                   .HasForeignKey(u => u.GroupId) 
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ug => ug.GroupPermissions)
                   .WithOne(gp => gp.Group) 
                   .HasForeignKey(gp => gp.GroupId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}