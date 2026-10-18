using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class PrepareTransformationRequestCommandHandlerTests
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
        var handler = new PrepareTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(new PrepareTransformationRequestCommand(99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }

    [Fact]
    public async Task A_request_that_is_already_prepared_is_a_failure()
    {
        var request = Request();
        request.Prepare(preparedByUserId: 7);

        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new PrepareTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(new PrepareTransformationRequestCommand(1), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request is already prepared.", result.Error);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task An_unauthenticated_caller_is_a_failure()
    {
        var uow = new FakeUnitOfWork { Requests = { Stored = Request() } };
        var handler = new PrepareTransformationRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = null });

        var result = await handler.Handle(new PrepareTransformationRequestCommand(1), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("An authenticated caller is required.", result.Error);
    }

    [Fact]
    public async Task A_requested_request_is_prepared_and_saved()
    {
        var request = Request();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new PrepareTransformationRequestCommandHandler(uow, new FakeCurrentUser { UserId = 42 });

        var result = await handler.Handle(new PrepareTransformationRequestCommand(1), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TransformationStatuses.Prepared, request.Status);
        Assert.Single(request.Preparations);
        Assert.Equal(1, uow.SaveCount);
    }
}
