namespace Rassef.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();
        public virtual DbSet<UserGroup> UserGroups => Set<UserGroup>();
        public virtual DbSet<Permission> Permissions => Set<Permission>();
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
        //action and status 
        public virtual DbSet<DriverTypes> DriverTypes => Set<DriverTypes>();
        public virtual DbSet<CommodityTypes> CommodityTypes => Set<CommodityTypes>();
        public virtual DbSet<DepartmentTypes> DepartmentTypes => Set<DepartmentTypes>();
        public virtual DbSet<DockStatuses> DockStatuses => Set<DockStatuses>();
        public virtual DbSet<ExitTypes> ExitTypes => Set<ExitTypes>();
        public virtual DbSet<PermitTypes> PermitTypes => Set<PermitTypes>();
        public virtual DbSet<QueueSettings> QueueSettings => Set<QueueSettings>();
        public virtual DbSet<RequestStatuses> RequestStatuses => Set<RequestStatuses>();
        public virtual DbSet<TicketStatuses> TicketStatuses => Set<TicketStatuses>();
        public virtual DbSet<TruckTypes> TruckTypes => Set<TruckTypes>();


        public ApplicationDbContext(DbContextOptions<DbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // apply all configuration 
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
