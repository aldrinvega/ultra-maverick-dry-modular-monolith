using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Manufacturing.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: the domain events raised by the aggregate are written to
/// Manufacturing.OutboxMessages in the same SaveChanges as the aggregate, so the shared
/// dispatcher can pick them up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_request_writes_one_TransformationRequested_row_and_its_lines()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);
        var planId = await TestData.CreateProductionPlanAsync(context);

        var requestId = await TestData.CreateRequestAsync(context, planId);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == requestId.ToString())
            .ToListAsync();

        var row = Assert.Single(rows);
        Assert.Equal("TransformationRequested", row.EventType);
        Assert.Equal("TransformationRequest", row.AggregateType);
        Assert.Null(row.PublishedAtUtc);
        Assert.Equal(0, row.Attempts);
        Assert.Contains("ChangeType", row.Payload);

        var lines = await context.TransformationRequestLines.AsNoTracking()
            .Where(x => x.TransformationRequestId == requestId)
            .ToListAsync();

        var line = Assert.Single(lines);
        Assert.Equal(10, line.ItemId);
        Assert.Equal(5m, line.RequiredQuantity);
    }

    [Fact]
    public async Task Preparing_a_request_writes_TransformationPrepared_and_a_preparation_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);
        var planId = await TestData.CreateProductionPlanAsync(context);
        var requestId = await TestData.CreateRequestAsync(context, planId);

        var request = await context.TransformationRequests
            .Include(x => x.Lines)
            .SingleAsync(x => x.Id == requestId);

        request.Prepare(preparedByUserId: 7);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.EventType == "TransformationPrepared")
            .ToListAsync();

        Assert.Contains(rows, x => x.AggregateId == requestId.ToString());

        var preparation = await context.TransformationRequestPreparations.AsNoTracking()
            .SingleAsync(x => x.TransformationRequestId == requestId);

        Assert.Equal(7, preparation.PreparedByUserId);
        Assert.Equal(5m, preparation.QuantityNeeded);
    }

    [Fact]
    public async Task Rejecting_a_request_writes_TransformationRejected_and_a_reject_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);
        var planId = await TestData.CreateProductionPlanAsync(context);
        var requestId = await TestData.CreateRequestAsync(context, planId);

        var request = await context.TransformationRequests.SingleAsync(x => x.Id == requestId);
        request.Reject(rejectReasonId: 3, quantity: 1m);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.EventType == "TransformationRejected")
            .ToListAsync();

        Assert.Contains(rows, x => x.AggregateId == requestId.ToString());

        var reject = await context.TransformationRequestRejects.AsNoTracking()
            .SingleAsync(x => x.TransformationRequestId == requestId);

        Assert.Equal(3, reject.ReasonId);
        Assert.Equal(1m, reject.Quantity);
    }
}
