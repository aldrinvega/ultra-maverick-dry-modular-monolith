using Microsoft.EntityFrameworkCore;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.Reporting.Persistence.Entities;

namespace Ultramaverick.Reporting.Persistence
{
    public sealed class ReportingDbContext : DbContext
    {
        public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

        public DbSet<InventoryPosition> InventoryPositions => Set<InventoryPosition>();
        public DbSet<OrderFulfillment> OrderFulfillment => Set<OrderFulfillment>();
        public DbSet<QcStatus> QcStatus => Set<QcStatus>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Reporting");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
