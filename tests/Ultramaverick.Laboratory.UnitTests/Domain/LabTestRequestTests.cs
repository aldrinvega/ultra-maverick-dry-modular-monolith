using Ultramaverick.Laboratory.Domain.Entities;
using Ultramaverick.Laboratory.Domain.Events;

namespace Ultramaverick.Laboratory.UnitTests.Domain;

public class LabTestRequestTests
{
    [Fact]
    public void Create_sets_requested_status_and_raises_created()
    {
        var requested = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var needed = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

        var request = LabTestRequest.Create(11, "LOT-1", 5m, "Micro", requested, needed, 7);

        Assert.Equal(11, request.WarehouseReceiptId);
        Assert.Equal("LOT-1", request.BatchNo);
        Assert.Equal(5m, request.Quantity);
        Assert.Equal("Micro", request.TestType);
        Assert.Equal(requested, request.DateRequested);
        Assert.Equal(needed, request.DateNeeded);
        Assert.Equal(7, request.RequestedByUserId);
        Assert.Equal("Requested", request.Status);
        Assert.True(request.IsActive);

        var raised = Assert.IsType<LabTestRequestChanged>(Assert.Single(request.DomainEvents));
        Assert.Equal("Created", raised.ChangeType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_quantity(decimal quantity)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => LabTestRequest.Create(null, null, quantity, null, null, null, null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_warehouse_receipt_id(int warehouseReceiptId)
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => LabTestRequest.Create(warehouseReceiptId, null, 1m, null, null, null, null));

    [Fact]
    public void Create_allows_a_missing_warehouse_receipt_id()
    {
        var request = LabTestRequest.Create(null, null, 1m, null, null, null, null);

        Assert.Null(request.WarehouseReceiptId);
    }

    [Fact]
    public void Tag_methods_add_one_join_row_per_id()
    {
        var request = LabTestRequest.Create(null, null, 1m, null, null, null, null);

        request.TagAnalyses(new[] { 1, 2 });
        request.TagParameters(new[] { 3 });
        request.TagProductConditions(new[] { 4 });
        request.TagSampleTypes(new[] { 5 });
        request.TagSwabTypes(new[] { 6, 7 });

        Assert.Equal(2, request.Analyses.Count);
        Assert.Single(request.Parameters);
        Assert.Single(request.ProductConditions);
        Assert.Single(request.SampleTypes);
        Assert.Equal(2, request.SwabTypes.Count);
        Assert.Contains(request.Analyses, x => x.AnalysisId == 1);
        Assert.Contains(request.Analyses, x => x.AnalysisId == 2);
        Assert.Contains(request.Parameters, x => x.ParameterId == 3);
        Assert.Contains(request.ProductConditions, x => x.ProductConditionId == 4);
        Assert.Contains(request.SampleTypes, x => x.SampleTypeId == 5);
        Assert.Contains(request.SwabTypes, x => x.SwabTypeId == 6);
        Assert.Contains(request.SwabTypes, x => x.SwabTypeId == 7);
    }
}
