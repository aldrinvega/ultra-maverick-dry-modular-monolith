using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on a Quality entity is written to
/// the module's Quality.OutboxMessages table in the same SaveChanges as the data change, so
/// the shared dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_qc_receipt_writes_one_QcReceiptChanged_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var receiptId = await TestData.CreateQcReceiptAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == receiptId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("QcReceiptChanged", rows[0].EventType);
        Assert.Equal("QcReceipt", rows[0].AggregateType);
        Assert.Equal(receiptId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }

    [Fact]
    public async Task Completing_a_qc_receipt_appends_a_QcReceiptCompleted_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var receiptId = await TestData.CreateQcReceiptAsync(context);
        await TestData.ClearOutboxAsync(context);

        var receipt = await context.QcReceipts.FirstAsync(x => x.Id == receiptId);
        receipt.Complete(7);
        await context.SaveChangesAsync();

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == receiptId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("QcReceiptCompleted", rows[0].EventType);
        Assert.Contains("\"CompletedBy\":7", rows[0].Payload);
    }
}
