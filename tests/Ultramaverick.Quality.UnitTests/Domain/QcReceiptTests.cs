using Ultramaverick.Quality.Contracts;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.Quality.Domain.Events;

namespace Ultramaverick.Quality.UnitTests.Domain;

public class QcReceiptTests
{
    [Fact]
    public void Create_sets_for_receiving_status_and_raises_created()
    {
        var receipt = QcReceipt.Create(1, 10, null, "LOT-1", 5m, 20, null);

        Assert.Equal(1, receipt.PurchaseOrderId);
        Assert.Equal(10, receipt.ItemId);
        Assert.Equal(20, receipt.UomId);
        Assert.Equal("LOT-1", receipt.BatchNo);
        Assert.Equal(5m, receipt.ActualDelivered);
        Assert.Equal("ForReceiving", receipt.Status);
        Assert.True(receipt.IsActive);

        var raised = Assert.IsType<QcReceiptChanged>(receipt.DomainEvents.First());
        Assert.Equal("Created", raised.ChangeType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_purchase_order_id(int purchaseOrderId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => QcReceipt.Create(purchaseOrderId, 10, null, null, 1m, 20, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_item_id(int itemId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => QcReceipt.Create(1, itemId, null, null, 1m, 20, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_uom_id(int uomId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => QcReceipt.Create(1, 10, null, null, 1m, uomId, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_purchase_order_line_id(int purchaseOrderLineId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => QcReceipt.Create(1, 10, null, null, 1m, 20, purchaseOrderLineId));

    [Fact]
    public void Create_binds_the_purchase_order_line_id_when_supplied()
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 1m, 20, 42);

        Assert.Equal(42, receipt.PurchaseOrderLineId);
    }

    [Fact]
    public void Reject_appends_a_reject_and_accumulates_total()
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 10m, 20, null);
        receipt.ClearDomainEvents();

        receipt.Reject(3m, null, "damaged", 7);

        Assert.Single(receipt.Rejects);
        Assert.Equal(3m, receipt.TotalReject);

        var raised = Assert.IsType<QcReceiptChanged>(receipt.DomainEvents.First());
        Assert.Equal("Rejected", raised.ChangeType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reject_rejects_non_positive_quantity(decimal quantity)
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 10m, 20, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => receipt.Reject(quantity, null, null, null));
    }

    [Fact]
    public void Reject_cannot_exceed_the_remaining_delivered_quantity()
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 2m, 20, null);
        receipt.Reject(2m, null, null, null);

        Assert.Throws<InvalidOperationException>(() => receipt.Reject(1m, null, null, null));
    }

    [Fact]
    public void Complete_sets_completed_and_raises_event_with_net_quantity()
    {
        var receipt = QcReceipt.Create(1, 10, null, "LOT-1", 10m, 20, null);
        receipt.Reject(4m, null, null, null);
        receipt.ClearDomainEvents();

        receipt.Complete(7);

        Assert.Equal("Completed", receipt.Status);
        Assert.Equal(7, receipt.QcByUserId);

        var raised = Assert.IsType<QcReceiptCompleted>(Assert.Single(receipt.DomainEvents));
        Assert.Equal(6m, raised.NetQuantity);
        Assert.Equal(10, raised.ItemId);
        Assert.Equal(20, raised.UomId);
        Assert.Equal("LOT-1", raised.LotNo);
        Assert.Equal(7, raised.CompletedBy);
    }

    [Fact]
    public void Complete_is_idempotent()
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 5m, 20, null);
        receipt.Complete(7);
        receipt.ClearDomainEvents();

        receipt.Complete(7);

        Assert.Equal("Completed", receipt.Status);
        Assert.Empty(receipt.DomainEvents);
    }

    [Fact]
    public void ApproveExpiry_is_a_no_op_when_not_nearly_expire()
    {
        var receipt = QcReceipt.Create(1, 10, null, null, 5m, 20, null);
        receipt.ClearDomainEvents();

        receipt.ApproveExpiry(7);

        Assert.False(receipt.IsNearlyExpire);
        Assert.Null(receipt.ExpiryApprovedByUserId);
        Assert.Empty(receipt.DomainEvents);
    }
}
