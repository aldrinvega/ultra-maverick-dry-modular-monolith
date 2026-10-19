using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Contracts;
using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class FinishTransformationRequestCommandHandlerTests
{
    private static TransformationRequest PreparedRequest()
    {
        var request = TransformationRequest.Create(
            productionPlanId: 1,
            warehouseId: 2,
            createdByUserId: 7,
            lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

        request.Approve(approvedByUserId: 7);
        request.Prepare(preparedByUserId: 7);
        request.ClearDomainEvents();
        return request;
    }

    [Fact]
    public async Task An_unknown_request_is_a_failure()
    {
        var handler = new FinishTransformationRequestCommandHandler(
            new FakeUnitOfWork(), new FakeCurrentUser());

        var result = await handler.Handle(
            new FinishTransformationRequestCommand(99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }

    [Fact]
    public async Task A_prepared_request_finishes_and_raises_one_event()
    {
        var request = PreparedRequest();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new FinishTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new FinishTransformationRequestCommand(1), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TransformationStatuses.Finished, request.Status);

        var raised = Assert.IsType<TransformationCompleted>(Assert.Single(request.DomainEvents));
        Assert.Equal(1, raised.ProductionPlanId);
        Assert.Equal(2, raised.WarehouseId);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task Finishing_twice_raises_one_event()
    {
        var request = PreparedRequest();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new FinishTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        await handler.Handle(new FinishTransformationRequestCommand(1), CancellationToken.None);
        await handler.Handle(new FinishTransformationRequestCommand(1), CancellationToken.None);

        Assert.Single(request.DomainEvents);
    }

    [Fact]
    public async Task A_requested_request_cannot_be_finished()
    {
        var request = TransformationRequest.Create(
            productionPlanId: 1,
            warehouseId: 2,
            createdByUserId: 7,
            lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new FinishTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new FinishTransformationRequestCommand(1), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }
}
