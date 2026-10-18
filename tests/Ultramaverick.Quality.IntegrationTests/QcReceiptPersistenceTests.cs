using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.IntegrationTests;

public class QcReceiptPersistenceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public QcReceiptPersistenceTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Rejects_round_trip_with_decimal_precision()
    {
        int receiptId;
        await using (var context = _fixture.CreateContext())
        {
            var receipt = QcReceipt.Create(1, 10, null, null, 10m, 20);
            receipt.Reject(3.1234m, 1, "damaged", 7);
            context.QcReceipts.Add(receipt);
            await context.SaveChangesAsync();
            receiptId = receipt.Id;
        }

        await using (var context = _fixture.CreateContext())
        {
            var reloaded = await context.QcReceipts
                .Include(x => x.Rejects)
                .SingleAsync(x => x.Id == receiptId);

            var reject = Assert.Single(reloaded.Rejects);
            Assert.Equal(3.1234m, reject.Quantity);
            Assert.Equal("damaged", reject.Remarks);
            Assert.Equal(3.1234m, reloaded.TotalReject);
        }
    }

    [Fact]
    public async Task Complete_raises_QcReceiptCompleted_with_the_net_quantity()
    {
        int receiptId;
        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);

            var receipt = QcReceipt.Create(1, 10, null, "LOT-1", 10m, 20);
            receipt.Reject(4m, null, null, null);
            context.QcReceipts.Add(receipt);
            await context.SaveChangesAsync();
            receiptId = receipt.Id;
        }

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);

            var receipt = await context.QcReceipts.FirstAsync(x => x.Id == receiptId);
            receipt.Complete(7);
            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.CreateContext())
        {
            var completed = await context.OutboxMessages.AsNoTracking()
                .Where(x => x.AggregateId == receiptId.ToString() && x.EventType == "QcReceiptCompleted")
                .ToListAsync();

            Assert.Single(completed);
            Assert.Contains("\"NetQuantity\":6", completed[0].Payload);

            var receipt = await context.QcReceipts.AsNoTracking().SingleAsync(x => x.Id == receiptId);
            Assert.Equal("Completed", receipt.Status);
        }
    }
}
