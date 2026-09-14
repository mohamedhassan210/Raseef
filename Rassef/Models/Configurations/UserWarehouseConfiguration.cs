namespace Rassef.Configurations
{
    public class UserWarehouseConfiguration : IEntityTypeConfiguration<UserWarehouse>
    {
        public void Configure(EntityTypeBuilder<UserWarehouse> builder)
        {
            builder.ToTable("UserWarehouses");

            builder.HasOne(uw => uw.User)
                   .WithMany(u => u.UserWarehouses)
                   .HasForeignKey(uw => uw.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(uw => uw.Warehouse)
                   .WithMany(w => w.UserWarehouses)
                   .HasForeignKey(uw => uw.WarehouseId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}