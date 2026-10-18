using Ultramaverick.Quality.Application.Commands.QcReceipts;
using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.Quality.UnitTests.Fakes;

namespace Ultramaverick.Quality.UnitTests.Application;

public class RejectQcReceiptCommandHandlerTests
{
    [Fact]
    public async Task Handle_rejects_the_quantity_and_saves_once()
    {
        var uow = new FakeQualityUnitOfWork();
        var receipt = QcReceipt.Create(1, 10, null, null, 10m, 20);
        uow.QcReceiptsRepo.SeedWithId(1, receipt);
        var handler = new RejectQcReceiptCommandHandler(uow, new FakeCurrentUser { UserId = 7 });

        var result = await handler.Handle(
            new RejectQcReceiptCommand(1, 3m, null, "damaged"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(3m, receipt.TotalReject);
        Assert.Single(receipt.Rejects);
    }

    [Fact]
    public async Task Handle_fails_for_an_unknown_receipt()
    {
        var uow = new FakeQualityUnitOfWork();
        var handler = new RejectQcReceiptCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new RejectQcReceiptCommand(99, 1m, null, null), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_fails_when_the_quantity_exceeds_the_remaining_delivered()
    {
        var uow = new FakeQualityUnitOfWork();
        var receipt = QcReceipt.Create(1, 10, null, null, 1m, 20);
        uow.QcReceiptsRepo.SeedWithId(1, receipt);
        var handler = new RejectQcReceiptCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new RejectQcReceiptCommand(1, 5m, null, null), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0m, receipt.TotalReject);
        Assert.Equal(0, uow.SaveCount);
    }
}
