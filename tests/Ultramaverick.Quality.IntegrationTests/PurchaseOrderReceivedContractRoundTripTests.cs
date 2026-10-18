using System.Text.Json;
using Ultramaverick.Procurement.Contracts;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Round-trips the real cross-module contract through the same System.Text.Json serialization the
/// outbox uses, asserting no field (including LineItemIds) is lost.
/// </summary>
public class PurchaseOrderReceivedContractRoundTripTests
{
    [Fact]
    public void The_contract_round_trips_through_the_outbox_serializer_without_field_loss()
    {
        var original = new PurchaseOrderReceived(
            PurchaseOrderId: 7,
            SupplierId: 5,
            LineItemIds: new[] { 100, 200, 300 },
            OccurredAtUtc: new DateTime(2026, 10, 18, 12, 30, 45, DateTimeKind.Utc));

        var payload = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<PurchaseOrderReceived>(payload);

        Assert.NotNull(restored);
        Assert.Equal(original.PurchaseOrderId, restored!.PurchaseOrderId);
        Assert.Equal(original.SupplierId, restored.SupplierId);
        Assert.Equal(original.LineItemIds, restored.LineItemIds);
        Assert.Equal(original.OccurredAtUtc, restored.OccurredAtUtc);
    }
}
