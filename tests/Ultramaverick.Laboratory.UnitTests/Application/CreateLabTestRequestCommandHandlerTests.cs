using Ultramaverick.Laboratory.Application.Commands.LabTestRequests;
using Ultramaverick.Laboratory.UnitTests.Fakes;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class CreateLabTestRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_the_request_with_its_tags_and_saves_once()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var handler = new CreateLabTestRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = 7 });

        var result = await handler.Handle(
            new CreateLabTestRequestCommand(
                11, "LOT-1", 5m, "Micro", null, null,
                new[] { 1, 2 }, new[] { 3 }, new[] { 4 }, new[] { 5 }, new[] { 6, 7 }),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var entity = Assert.Single(uow.LabTestRequestsRepo.All);
        Assert.Equal(11, entity.WarehouseReceiptId);
        Assert.Equal("LOT-1", entity.BatchNo);
        Assert.Equal(5m, entity.Quantity);
        Assert.Equal(7, entity.RequestedByUserId);
        Assert.Equal(2, entity.Analyses.Count);
        Assert.Single(entity.Parameters);
        Assert.Single(entity.ProductConditions);
        Assert.Single(entity.SampleTypes);
        Assert.Equal(2, entity.SwabTypes.Count);
    }

    [Fact]
    public async Task Handle_treats_null_tag_arrays_as_empty()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var handler = new CreateLabTestRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new CreateLabTestRequestCommand(
                11, "LOT-1", 5m, "Micro", null, null, null, null, null, null, null),
            CancellationToken.None);

        Assert.True(result.Succeeded);

        var entity = Assert.Single(uow.LabTestRequestsRepo.All);
        Assert.Empty(entity.Analyses);
        Assert.Empty(entity.Parameters);
        Assert.Empty(entity.ProductConditions);
        Assert.Empty(entity.SampleTypes);
        Assert.Empty(entity.SwabTypes);
    }
}
