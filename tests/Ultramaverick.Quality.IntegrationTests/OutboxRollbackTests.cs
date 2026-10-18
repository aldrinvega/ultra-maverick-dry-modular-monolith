using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Domain.Entities;

namespace Ultramaverick.Quality.IntegrationTests;

/// <summary>
/// Proves the QC-receipt insert and its outbox write are a single atomic unit of work: when the
/// outbox flush fails after the receipt has already been inserted, the whole unit rolls back, so
/// neither the receipt nor an outbox row is persisted.
/// </summary>
public class OutboxRollbackTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OutboxRollbackTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task A_failed_outbox_flush_rolls_back_the_qc_receipt_insert()
    {
        var batchNo = TestData.Code("LOT");

        await using (var context = _fixture.CreateContext())
        {
            await TestData.ClearOutboxAsync(context);
        }

        await using (var context = _fixture.CreateContext(new FailingOutboxCommandInterceptor()))
        {
            context.QcReceipts.Add(QcReceipt.Create(1, 10, null, batchNo, 5m, 20));

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.IsType<InvalidOperationException>(exception.InnerException);
        }

        await using (var context = _fixture.CreateContext())
        {
            Assert.Empty(await context.QcReceipts.AsNoTracking()
                .Where(x => x.BatchNo == batchNo).ToListAsync());
            Assert.Empty(await context.OutboxMessages.AsNoTracking().ToListAsync());
        }
    }
}
