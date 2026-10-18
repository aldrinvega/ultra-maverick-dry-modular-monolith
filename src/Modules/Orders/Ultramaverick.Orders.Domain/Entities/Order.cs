using Ultramaverick.Orders.Contracts;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Orders.Domain.Entities;

public sealed class Order : Entity
{
    private readonly List<OrderLine> _lines = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public int CustomerId { get; private set; }             // scalar → Catalog (no FK)
    public string? CustomerPosition { get; private set; }
    public int? FarmId { get; private set; }                // scalar → Catalog (no FK)
    public DateTime? OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public TimeOnly? TimeNeeded { get; private set; }
    public int? TransactionTypeId { get; private set; }     // scalar → Catalog (no FK)
    public string Status { get; private set; } = OrderStatuses.Draft;
    public int? PreparedByUserId { get; private set; }
    public DateTime? PreparedAtUtc { get; private set; }
    public int? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public int? RejectedByUserId { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public int? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public int? ReasonId { get; private set; }              // scalar → Catalog (no FK)
    public int? SetByUserId { get; private set; }
    public bool IsMove { get; private set; }
    public bool IsCancelledOrder { get; private set; }
    public string? Remarks { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { }

    public static Order Create(
        string orderNumber, int customerId, DateTime? orderDate, DateTime? deliveryDate,
        string? remarks, int? createdByUserId, IEnumerable<OrderLineInput> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            OrderDate = orderDate,
            DeliveryDate = deliveryDate,
            Remarks = remarks,
            Status = OrderStatuses.Draft,
            CreatedByUserId = createdByUserId
        };

        // OrderLine.Create takes no parent id: EF wires the FK from the navigation on save.
        foreach (var line in lines)
            order._lines.Add(OrderLine.Create(line.ItemId, line.UomId, line.Quantity, line.FormulaHeaderId));

        order.RaiseDomainEvent(new OrderCreated(orderNumber, customerId, DateTime.UtcNow));
        return order;
    }

    public void Confirm()
    {
        Status = OrderStatuses.Confirmed;
        ModifiedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new OrderConfirmed(OrderNumber, DateTime.UtcNow));
    }

    public void Cancel(string? reason, int? userId)
    {
        var occurredAt = DateTime.UtcNow;
        Status = OrderStatuses.Cancelled;
        IsCancelledOrder = true;
        CancelledAtUtc = occurredAt;
        CancelledByUserId = userId;
        ModifiedAtUtc = occurredAt;
        ModifiedByUserId = userId;
        RaiseDomainEvent(new OrderCancelled(OrderNumber, reason, occurredAt));
    }
}
