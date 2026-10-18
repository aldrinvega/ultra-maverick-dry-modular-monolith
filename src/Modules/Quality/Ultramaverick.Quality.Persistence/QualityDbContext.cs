using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.Quality.Persistence.Entities;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Persistence
{
    public sealed class QualityDbContext : DbContext
    {
        public QualityDbContext(DbContextOptions<QualityDbContext> options) : base(options) { }

        public DbSet<QcReceipt> QcReceipts => Set<QcReceipt>();
        public DbSet<QcReject> QcRejects => Set<QcReject>();
        public DbSet<ChecklistType> ChecklistTypes => Set<ChecklistType>();
        public DbSet<ChecklistQuestion> ChecklistQuestions => Set<ChecklistQuestion>();
        public DbSet<Checklist> Checklists => Set<Checklist>();
        public DbSet<ChecklistAnswer> ChecklistAnswers => Set<ChecklistAnswer>();
        public DbSet<ChecklistOpenFieldAnswer> ChecklistOpenFieldAnswers => Set<ChecklistOpenFieldAnswer>();
        public DbSet<ChecklistProductDimension> ChecklistProductDimensions => Set<ChecklistProductDimension>();
        public DbSet<ChecklistOtherObservation> ChecklistOtherObservations => Set<ChecklistOtherObservation>();
        public DbSet<ChecklistCompliance> ChecklistCompliances => Set<ChecklistCompliance>();
        public DbSet<ChecklistReviewVerificationLog> ChecklistReviewVerificationLogs => Set<ChecklistReviewVerificationLog>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Quality");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(QualityDbContext).Assembly);
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
