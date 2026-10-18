using Ultramaverick.Warehouse.Contracts;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.UnitTests.Domain;

public class WarehouseReceiptTests
{
    [Fact]
    public void Create_sets_fields_and_raises_stock_received()
    {
        var receipt = WarehouseReceipt.Create(
            qcReceiptId: 55, warehouseId: 1, itemId: 10, uomId: 20,
            lotNo: "LOT-1", quantity: 5m,
            manufacturingDate: null, expiryDate: null, receivedBy: 7);

        Assert.Equal(55, receipt.QcReceiptId);
        Assert.Equal(1, receipt.WarehouseId);
        Assert.Equal(10, receipt.ItemId);
        Assert.Equal(20, receipt.UomId);
        Assert.Equal("LOT-1", receipt.LotNo);
        Assert.Equal(5m, receipt.Quantity);
        Assert.True(receipt.IsActive);

        var raised = Assert.IsType<StockReceived>(Assert.Single(receipt.DomainEvents));
        Assert.Equal(1, raised.WarehouseId);
        Assert.Equal(10, raised.ItemId);
        Assert.Equal(20, raised.UomId);
        Assert.Equal("LOT-1", raised.LotNo);
        Assert.Equal(5m, raised.Quantity);
    }

    [Fact]
    public void Create_allows_a_null_qc_receipt_id_for_a_manual_receipt()
    {
        var receipt = WarehouseReceipt.Create(null, 1, 10, 20, null, 1m, null, null, 7);

        Assert.Null(receipt.QcReceiptId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_non_positive_warehouse_id(int warehouseId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => WarehouseReceipt.Create(null, warehouseId, 10, 20, null, 1m, null, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_non_positive_item_id(int itemId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => WarehouseReceipt.Create(null, 1, itemId, 20, null, 1m, null, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_non_positive_uom_id(int uomId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => WarehouseReceipt.Create(null, 1, 10, uomId, null, 1m, null, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_non_positive_quantity(decimal quantity)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => WarehouseReceipt.Create(null, 1, 10, 20, null, quantity, null, null, null));

    [Fact]
    public void StockReceived_carries_no_id()
    {
        var receipt = WarehouseReceipt.Create(null, 1, 10, 20, null, 1m, null, null, null);

        var raised = Assert.IsType<StockReceived>(Assert.Single(receipt.DomainEvents));
        Assert.Null(raised.GetType().GetProperty("Id"));
    }
}
