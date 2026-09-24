namespace Ultramaverick.SharedKernel
{
    public interface IDomainEvent
    {
        DateTime OccurredAtUtc { get; }
    }

}
