using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Events;

public sealed record QcReceiptChanged(string ChangeType, DateTime OccurredAtUtc) : IDomainEvent;
