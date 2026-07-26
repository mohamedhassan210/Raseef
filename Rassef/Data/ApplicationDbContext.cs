using System.Reflection;

namespace Rassef.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<CheckOut> CheckOuts => Set<CheckOut>();
        public virtual DbSet<Department> Departments => Set<Department>();
        public virtual DbSet<Dock> Docks => Set<Dock>();
        public virtual DbSet<DockAssignment> DockAssignments => Set<DockAssignment>();
        public virtual DbSet<Driver> Drivers => Set<Driver>();
        public virtual DbSet<QueueAction> QueueActions => Set<QueueAction>();
        public virtual DbSet<QueueTicket> QueueTickets => Set<QueueTicket>();
        public virtual DbSet<Supplier> Suppliers => Set<Supplier>();
        public virtual DbSet<SupplierRequest> SupplierRequests => Set<SupplierRequest>();
        public virtual DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
        public virtual DbSet<Truck> Trucks => Set<Truck>();
        public virtual DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public ApplicationDbContext(DbContextOptions<DbContext> options) : base (options){}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // apply all configuration 
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
