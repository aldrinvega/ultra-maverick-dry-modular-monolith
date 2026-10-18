using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Domain.Entities;
using Ultramaverick.Laboratory.Persistence.Entities;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Persistence
{
    public sealed class LaboratoryDbContext : DbContext
    {
        public LaboratoryDbContext(DbContextOptions<LaboratoryDbContext> options) : base(options) { }

        public DbSet<LabTestRequest> LabTestRequests => Set<LabTestRequest>();
        public DbSet<LabTestAnalysis> LabTestAnalyses => Set<LabTestAnalysis>();
        public DbSet<LabTestParameter> LabTestParameters => Set<LabTestParameter>();
        public DbSet<LabTestProductCondition> LabTestProductConditions => Set<LabTestProductCondition>();
        public DbSet<LabTestSampleType> LabTestSampleTypes => Set<LabTestSampleType>();
        public DbSet<LabTestSwabType> LabTestSwabTypes => Set<LabTestSwabType>();
        public DbSet<LabReceiveRequest> LabReceiveRequests => Set<LabReceiveRequest>();
        public DbSet<LabRejectedItem> LabRejectedItems => Set<LabRejectedItem>();
        public DbSet<LabReturnedItem> LabReturnedItems => Set<LabReturnedItem>();
        public DbSet<LabTestResult> LabTestResults => Set<LabTestResult>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Laboratory");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaboratoryDbContext).Assembly);
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
