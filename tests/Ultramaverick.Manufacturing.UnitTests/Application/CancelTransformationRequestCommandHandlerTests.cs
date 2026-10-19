using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class CancelTransformationRequestCommandHandlerTests
{
    private static TransformationRequest Request() => TransformationRequest.Create(
        productionPlanId: 1,
        warehouseId: 2,
        createdByUserId: 7,
        lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

    [Fact]
    public async Task An_unknown_request_is_a_failure()
    {
        var handler = new CancelTransformationRequestCommandHandler(
            new FakeUnitOfWork(), new FakeCurrentUser());

        var result = await handler.Handle(
            new CancelTransformationRequestCommand(99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }

    [Fact]
    public async Task A_request_is_cancelled_and_saved_with_the_callers_id()
    {
        var request = Request();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new CancelTransformationRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = 42 });

        var result = await handler.Handle(
            new CancelTransformationRequestCommand(1), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TransformationStatuses.Cancelled, request.Status);
        Assert.Equal(42, request.ModifiedByUserId);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task Cancelling_twice_raises_only_one_event()
    {
        var request = Request();
        request.ClearDomainEvents();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new CancelTransformationRequestCommandHandler(
            uow, new FakeCurrentUser());

        await handler.Handle(new CancelTransformationRequestCommand(1), CancellationToken.None);
        await handler.Handle(new CancelTransformationRequestCommand(1), CancellationToken.None);

        Assert.Single(request.DomainEvents);
    }
}
