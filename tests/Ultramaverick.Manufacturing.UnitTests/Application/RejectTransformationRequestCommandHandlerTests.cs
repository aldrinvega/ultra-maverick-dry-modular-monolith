using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class RejectTransformationRequestCommandHandlerTests
{
    private static TransformationRequest Request() => TransformationRequest.Create(
        productionPlanId: 1,
        warehouseId: 2,
        createdByUserId: 7,
        lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

    [Fact]
    public async Task An_unknown_request_is_a_failure()
    {
        var uow = new FakeUnitOfWork();
        var handler = new RejectTransformationRequestCommandHandler(uow);

        var result = await handler.Handle(
            new RejectTransformationRequestCommand(99, 3, 1m), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }

    [Fact]
    public async Task A_prepared_request_cannot_be_rejected()
    {
        var request = Request();
        request.Prepare(preparedByUserId: 7);

        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new RejectTransformationRequestCommandHandler(uow);

        var result = await handler.Handle(
            new RejectTransformationRequestCommand(1, 3, 1m), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_requested_request_is_rejected_and_saved()
    {
        var request = Request();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new RejectTransformationRequestCommandHandler(uow);

        var result = await handler.Handle(
            new RejectTransformationRequestCommand(1, 3, 1.5m), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(request.Rejects);
        Assert.Equal(TransformationStatuses.Requested, request.Status);
        Assert.Equal(1, uow.SaveCount);
    }
}
