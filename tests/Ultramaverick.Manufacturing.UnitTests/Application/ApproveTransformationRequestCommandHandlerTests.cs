using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Domain;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class ApproveTransformationRequestCommandHandlerTests
{
    private static TransformationRequest Request() => TransformationRequest.Create(
        productionPlanId: 1,
        warehouseId: 2,
        createdByUserId: 7,
        lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

    [Fact]
    public async Task An_unknown_request_is_a_failure()
    {
        var handler = new ApproveTransformationRequestCommandHandler(
            new FakeUnitOfWork(), new FakeCurrentUser());

        var result = await handler.Handle(
            new ApproveTransformationRequestCommand(99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }

    [Fact]
    public async Task A_requested_request_is_approved_with_the_callers_id()
    {
        var request = Request();
        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new ApproveTransformationRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = 42 });

        var result = await handler.Handle(
            new ApproveTransformationRequestCommand(1), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TransformationStatuses.Approved, request.Status);
        Assert.Equal(42, request.ModifiedByUserId);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task A_prepared_request_cannot_be_approved()
    {
        var request = Request();
        request.Prepare(preparedByUserId: 7);

        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new ApproveTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new ApproveTransformationRequestCommand(1), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task An_approved_request_can_still_be_prepared()
    {
        var request = Request();
        request.Approve(approvedByUserId: 7);

        var uow = new FakeUnitOfWork { Requests = { Stored = request } };
        var handler = new PrepareTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new PrepareTransformationRequestCommand(1), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TransformationStatuses.Prepared, request.Status);
    }
}
