using Ultramaverick.Procurement.Domain.Entities;
using Ultramaverick.Procurement.Domain.Events;

namespace Ultramaverick.Procurement.UnitTests.Domain;

public class PurchaseOrderTests
{
    [Fact]
    public void Create_sets_open_status_and_raises_created()
    {
        var po = PurchaseOrder.Create("PO-1", "PR-1", null, null, 5, null, 7);

        Assert.Equal("PO-1", po.PoNumber);
        Assert.Equal("Open", po.Status);
        Assert.True(po.IsActive);
        Assert.Equal(7, po.CreatedByUserId);

        var raised = Assert.IsType<PurchaseOrderChanged>(po.DomainEvents.First());
        Assert.Equal("Created", raised.ChangeType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_po_number(string poNumber)
        => Assert.Throws<ArgumentException>(
            () => PurchaseOrder.Create(poNumber, null, null, null, 1, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_supplier(int supplierId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => PurchaseOrder.Create("PO-1", null, null, null, supplierId, null, null));

    [Fact]
    public void AddLine_appends_line_and_raises_line_added()
    {
        var po = PurchaseOrder.Create("PO-1", null, null, null, 1, null, null);
        po.ClearDomainEvents();

        var line = po.AddLine(10, 20, 3m, 4.5m);

        Assert.Single(po.Lines);
        Assert.Equal(10, line.ItemId);
        Assert.Equal(20, line.UomId);
        Assert.Equal(3m, line.OrderedQuantity);
        Assert.Equal(4.5m, line.UnitPrice);

        var raised = Assert.IsType<PurchaseOrderChanged>(po.DomainEvents.First());
        Assert.Equal("LineAdded", raised.ChangeType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddLine_rejects_non_positive_quantity(decimal quantity)
    {
        var po = PurchaseOrder.Create("PO-1", null, null, null, 1, null, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => po.AddLine(10, 20, quantity, 1m));
    }

    [Fact]
    public void MarkReceived_sets_received_status_and_raises_received()
    {
        var po = PurchaseOrder.Create("PO-1", null, null, null, 5, null, null);
        po.ClearDomainEvents();

        po.MarkReceived(9);

        Assert.Equal("Received", po.Status);
        Assert.Equal(9, po.ModifiedByUserId);

        var raised = Assert.IsType<PurchaseOrderReceived>(po.DomainEvents.First());
        Assert.Equal(po.Id, raised.PurchaseOrderId);
        Assert.Equal(5, raised.SupplierId);
    }

    [Fact]
    public void MarkReceived_is_idempotent_when_already_received()
    {
        var po = PurchaseOrder.Create("PO-1", null, null, null, 5, null, null);
        po.MarkReceived(9);
        po.ClearDomainEvents();

        po.MarkReceived(9);

        Assert.Equal("Received", po.Status);
        Assert.Empty(po.DomainEvents);
    }
}
