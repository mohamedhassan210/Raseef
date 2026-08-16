namespace Rassef.Models.Configurations
{
    public class DepartmentTypeConfiguration : IEntityTypeConfiguration<DepartmentTypes>
    {
        public void Configure(EntityTypeBuilder<DepartmentTypes> builder)
        {
            builder.ToTable("DepartmentTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.Departments)
                   .WithOne(x => x.DepartmentType)
                   .HasForeignKey(x => x.DepartmentTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}