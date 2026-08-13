namespace Rassef.Models.Configurations
{
    public class DepartmentTypeConfiguration : IEntityTypeConfiguration<DepartmentType>
    {
        public void Configure(EntityTypeBuilder<DepartmentType> builder)
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