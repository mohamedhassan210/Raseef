using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rassef.Models.Common;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.StatusesAndActions;

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
        public virtual DbSet<Position> Positions => Set<Position>();
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
        public virtual DbSet<Shift> Shifts => Set<Shift>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global Query Filter for Soft Delete on all BaseEntity types
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProp = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var compareExpression = Expression.Equal(isDeletedProp, Expression.Constant(false));
                    var filter = Expression.Lambda(compareExpression, parameter);
                    entityType.SetQueryFilter(filter);
                }
            }

            modelBuilder.Entity<TruckTypes>()
                .HasIndex(t => t.TruckTypeCode).IsUnique();

            modelBuilder.Entity<DriverTypes>()
                .HasIndex(d => d.Code).IsUnique();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDeleteRules();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplySoftDeleteRules();
            return base.SaveChanges();
        }

        private void ApplySoftDeleteRules()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedAT == default)
                    {
                        entry.Entity.CreatedAT = DateTimeOffset.Now;
                    }
                    if (entry.Entity.UpdatedAT == default)
                    {
                        entry.Entity.UpdatedAT = DateTimeOffset.Now;
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.MarkAsUpdated();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }
        }
    }
}
