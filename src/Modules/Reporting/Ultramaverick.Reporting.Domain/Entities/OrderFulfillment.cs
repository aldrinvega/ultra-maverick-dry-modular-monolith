namespace Ultramaverick.Reporting.Domain.Entities;

/// <summary>The latest known status of one order, keyed by order number.</summary>
public sealed class OrderFulfillment
{
    public string OrderNumber { get; private set; } = null!;
    public int? CustomerId { get; private set; }
    public string Status { get; private set; } = ReportingStatuses.Created;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private OrderFulfillment() { }

    public static OrderFulfillment Create(string orderNumber, int? customerId, DateTime occurredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        return new OrderFulfillment
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            Status = ReportingStatuses.Created,
            OccurredAtUtc = occurredAtUtc,
            UpdatedAtUtc = occurredAtUtc
        };
    }

    public void Confirm(DateTime occurredAtUtc)
    {
        Status = ReportingStatuses.Confirmed;
        UpdatedAtUtc = occurredAtUtc;
    }

    public void Cancel(DateTime occurredAtUtc)
    {
        Status = ReportingStatuses.Cancelled;
        UpdatedAtUtc = occurredAtUtc;
    }
}
