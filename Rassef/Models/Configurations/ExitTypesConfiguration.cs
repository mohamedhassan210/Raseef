namespace Rassef.Models.Configurations
{
    public class ExitTypesConfiguration : IEntityTypeConfiguration<ExitTypes>
    {
        public void Configure(EntityTypeBuilder<ExitTypes> builder)
        {
            builder.ToTable("ExitTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.CheckOuts)
                   .WithOne(x => x.ExitType)
                   .HasForeignKey(x => x.ExitTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}