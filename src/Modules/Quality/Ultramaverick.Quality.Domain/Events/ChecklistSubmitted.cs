using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Events;

public sealed record ChecklistSubmitted(int ChecklistId, int QcReceiptId, DateTime OccurredAtUtc) : IDomainEvent;
