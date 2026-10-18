using Ultramaverick.Reporting.Domain;
using Ultramaverick.Reporting.Domain.Entities;

namespace Ultramaverick.Reporting.UnitTests.Domain;

public class ReadModelTests
{
    private static readonly DateTime OccurredAt = new(2026, 10, 18, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Inventory_position_accumulates_on_hand_and_stamps_the_time()
    {
        var position = InventoryPosition.Create(1, 10, 20, "LOT-1", OccurredAt);

        Assert.Equal(0m, position.Quantity);

        position.AddOnHand(5m, OccurredAt.AddMinutes(1));
        position.AddOnHand(2.5m, OccurredAt.AddMinutes(2));

        Assert.Equal(7.5m, position.Quantity);
        Assert.Equal(OccurredAt.AddMinutes(2), position.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void An_absent_lot_is_normalised_to_the_empty_string(string? lotNo)
        => Assert.Equal(string.Empty, InventoryPosition.NormalizeLot(lotNo));

    [Fact]
    public void Adding_a_negative_quantity_throws()
    {
        var position = InventoryPosition.Create(1, 10, 20, null, OccurredAt);

        Assert.Throws<ArgumentOutOfRangeException>(() => position.AddOnHand(-1m, OccurredAt));
    }

    [Fact]
    public void Order_fulfillment_moves_from_created_to_confirmed_to_cancelled()
    {
        var fulfillment = OrderFulfillment.Create("SO-1", 7, OccurredAt);
        Assert.Equal(ReportingStatuses.Created, fulfillment.Status);

        fulfillment.Confirm(OccurredAt.AddMinutes(1));
        Assert.Equal(ReportingStatuses.Confirmed, fulfillment.Status);
        Assert.Equal(OccurredAt.AddMinutes(1), fulfillment.UpdatedAtUtc);

        fulfillment.Cancel(OccurredAt.AddMinutes(2));
        Assert.Equal(ReportingStatuses.Cancelled, fulfillment.Status);
    }

    [Fact]
    public void Qc_status_is_completed_and_can_be_updated()
    {
        var status = QcStatus.Create(5, 10, 20, "LOT-1", 100m, OccurredAt);
        Assert.Equal(ReportingStatuses.Completed, status.Status);
        Assert.Equal(100m, status.NetQuantity);

        status.Update(90m, OccurredAt.AddMinutes(5));

        Assert.Equal(90m, status.NetQuantity);
        Assert.Equal(OccurredAt.AddMinutes(5), status.UpdatedAtUtc);
    }
}
