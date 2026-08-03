namespace Rassef.Models.Configurations
{
    public class ActionTypesConfiguration : IEntityTypeConfiguration<ActionTypes>
    {
        public void Configure(EntityTypeBuilder<ActionTypes> builder)
        {
            builder.ToTable("ActionTypes");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(x => x.QueueActions)
                   .WithOne(x => x.ActionType)
                   .HasForeignKey(x => x.ActionTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}