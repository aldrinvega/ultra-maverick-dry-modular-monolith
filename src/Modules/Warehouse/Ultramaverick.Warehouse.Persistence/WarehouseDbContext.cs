using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.SharedKernel;
using Ultramaverick.Warehouse.Domain.Entities;
using Ultramaverick.Warehouse.Persistence.Entities;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.Persistence
{
    public sealed class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

        public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();
        public DbSet<WarehouseReceipt> WarehouseReceipts => Set<WarehouseReceipt>();
        public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
        public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
        public DbSet<StockReservation> StockReservations => Set<StockReservation>();
        public DbSet<MiscellaneousReceipt> MiscellaneousReceipts => Set<MiscellaneousReceipt>();
        public DbSet<MiscellaneousReceiptLine> MiscellaneousReceiptLines => Set<MiscellaneousReceiptLine>();
        public DbSet<MiscellaneousIssue> MiscellaneousIssues => Set<MiscellaneousIssue>();
        public DbSet<MiscellaneousIssueLine> MiscellaneousIssueLines => Set<MiscellaneousIssueLine>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Warehouse");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var ownsTransaction = Database.CurrentTransaction is null;
            var transaction = ownsTransaction
                ? await Database.BeginTransactionAsync(cancellationToken)
                : Database.CurrentTransaction!;

            try
            {
                // First save assigns identity values and applies entity changes.
                var result = await base.SaveChangesAsync(cancellationToken);

                var pending = ChangeTracker.Entries()
                    .Where(e => e.Entity is Entity)
                    .Select(e => new
                    {
                        Entry = e,
                        Entity = (Entity)e.Entity,
                        AggregateId = e.Property("Id").CurrentValue?.ToString() ?? string.Empty
                    })
                    .Where(x => x.Entity.DomainEvents.Count > 0)
                    .ToList();

                foreach (var item in pending)
                {
                    foreach (var domainEvent in item.Entity.DomainEvents)
                    {
                        OutboxMessages.Add(OutboxMessage.Create(
                            aggregateType: item.Entry.Metadata.ClrType.Name,
                            aggregateId: item.AggregateId,
                            eventType: domainEvent.GetType().Name,
                            payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType())));
                    }

                    item.Entity.ClearDomainEvents();
                }

                // Second save writes the outbox rows inside the same transaction.
                if (pending.Count > 0)
                    await base.SaveChangesAsync(cancellationToken);

                if (ownsTransaction)
                    await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                if (ownsTransaction)
                    await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        }
    }
}
