using Microsoft.EntityFrameworkCore;

namespace Ultramaverick.Laboratory.IntegrationTests;

/// <summary>
/// Proves the transactional outbox: a domain event raised on a Laboratory entity is written to
/// the module's Laboratory.OutboxMessages table in the same SaveChanges as the data change, so
/// the shared dispatcher can pick it up.
/// </summary>
public class OutboxTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_lab_test_request_writes_one_LabTestRequestChanged_row()
    {
        await using var context = _fixture.CreateContext();
        await TestData.ClearOutboxAsync(context);

        var requestId = await TestData.CreateLabTestRequestAsync(context);

        var rows = await context.OutboxMessages.AsNoTracking()
            .Where(x => x.AggregateId == requestId.ToString())
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal("LabTestRequestChanged", rows[0].EventType);
        Assert.Equal("LabTestRequest", rows[0].AggregateType);
        Assert.Equal(requestId.ToString(), rows[0].AggregateId);
        Assert.Null(rows[0].PublishedAtUtc);
        Assert.Equal(0, rows[0].Attempts);
        Assert.Contains("Created", rows[0].Payload);
    }
}
