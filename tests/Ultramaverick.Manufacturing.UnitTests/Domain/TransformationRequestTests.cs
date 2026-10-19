using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;
using Ultramaverick.Manufacturing.Contracts;

namespace Ultramaverick.Manufacturing.UnitTests.Domain;

public class TransformationRequestTests
{
    private static TransformationRequest NewRequest() => TransformationRequest.Create(
        productionPlanId: 1,
        warehouseId: 2,
        createdByUserId: 7,
        lines: new[]
        {
            new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m),
            new TransformationLineInput(ItemId: 11, RequiredQuantity: 2.5m)
        });

    [Fact]
    public void Create_sets_requested_status_and_raises_one_id_free_event()
    {
        var request = NewRequest();

        Assert.Equal(TransformationStatuses.Requested, request.Status);
        Assert.Equal(2, request.Lines.Count);

        var raised = Assert.IsType<TransformationRequested>(Assert.Single(request.DomainEvents));
        Assert.Equal(TransformationChangeTypes.Created, raised.ChangeType);
        Assert.True(raised.OccurredAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_without_lines_throws()
    {
        Assert.Throws<ArgumentException>(() => TransformationRequest.Create(
            productionPlanId: 1,
            warehouseId: 2,
            createdByUserId: null,
            lines: Array.Empty<TransformationLineInput>()));
    }

    [Fact]
    public void Prepare_sets_prepared_status_and_records_the_required_quantity()
    {
        var request = NewRequest();
        request.ClearDomainEvents();

        request.Prepare(preparedByUserId: 7);

        Assert.Equal(TransformationStatuses.Prepared, request.Status);

        Assert.Equal(2, request.Preparations.Count);
        Assert.All(request.Preparations, p => Assert.Equal(7, p.PreparedByUserId));
        Assert.Equal(7.5m, request.Preparations.Sum(p => p.QuantityNeeded));

        var raised = Assert.IsType<TransformationPrepared>(Assert.Single(request.DomainEvents));
        Assert.Equal(TransformationChangeTypes.Prepared, raised.ChangeType);
    }

    [Fact]
    public void Reject_raises_its_own_event_and_appends_a_reject_row()
    {
        var request = NewRequest();
        request.ClearDomainEvents();

        request.Reject(rejectReasonId: 3, quantity: 1.5m);

        // The guide re-raised TransformationRequested here; the wave fixed that defect.
        var raised = Assert.IsType<TransformationRejected>(Assert.Single(request.DomainEvents));
        Assert.Equal(TransformationChangeTypes.Rejected, raised.ChangeType);

        var reject = Assert.Single(request.Rejects);
        Assert.Equal(3, reject.ReasonId);
        Assert.Equal(1.5m, reject.Quantity);

        // Reject does not transition the status.
        Assert.Equal(TransformationStatuses.Requested, request.Status);
    }

    [Fact]
    public void Reject_with_a_non_positive_quantity_throws()
    {
        var request = NewRequest();

        Assert.Throws<ArgumentOutOfRangeException>(() => request.Reject(3, 0m));
    }

    [Fact]
    public void Reject_after_prepare_throws()
    {
        var request = NewRequest();
        request.Prepare(preparedByUserId: 7);

        Assert.Throws<InvalidOperationException>(() => request.Reject(3, 1m));
    }
}

