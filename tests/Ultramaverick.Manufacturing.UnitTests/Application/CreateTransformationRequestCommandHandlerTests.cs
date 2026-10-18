using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Domain.Entities;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

public class CreateTransformationRequestCommandHandlerTests
{
    private static CreateTransformationRequestCommand Command() => new(
        ProductionPlanId: 1,
        WarehouseId: 2,
        Lines: new[] { new TransformationLineInput(ItemId: 10, RequiredQuantity: 5m) });

    [Fact]
    public async Task A_missing_production_plan_is_a_failure_and_nothing_is_added()
    {
        var uow = new FakeUnitOfWork();
        uow.Plans.Exists = false;

        var handler = new CreateTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Production plan not found.", result.Error);
        Assert.Null(uow.Requests.Added);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task A_request_without_lines_is_a_failure()
    {
        var uow = new FakeUnitOfWork();
        var handler = new CreateTransformationRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new CreateTransformationRequestCommand(1, 2, Array.Empty<TransformationLineInput>()),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("A transformation request must have at least one line.", result.Error);
        Assert.Null(uow.Requests.Added);
    }

    [Fact]
    public async Task A_valid_request_is_added_and_saved_with_the_callers_id()
    {
        var uow = new FakeUnitOfWork();
        var handler = new CreateTransformationRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = 42 });

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.True(result.Succeeded);

        var added = Assert.IsType<TransformationRequest>(uow.Requests.Added);
        Assert.Equal(42, added.CreatedByUserId);
        Assert.Equal(1, uow.SaveCount);
    }
}
