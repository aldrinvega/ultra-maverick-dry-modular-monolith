using Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;
using Ultramaverick.Laboratory.Domain.Entities;
using Ultramaverick.Laboratory.UnitTests.Fakes;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class AcceptLabReceiveRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_an_accepted_receive_request_and_saves_once()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var testRequest = LabTestRequest.Create(11, "LOT-1", 5m, "Micro", null, null, 7);
        uow.LabTestRequestsRepo.SeedWithId(10, testRequest);
        var handler = new AcceptLabReceiveRequestCommandHandler(
            uow, new FakeCurrentUser { UserId = 7 });

        var result = await handler.Handle(
            new AcceptLabReceiveRequestCommand(10, 3, null, "ok"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var receive = Assert.Single(uow.LabReceiveRequestsRepo.All);
        Assert.Equal(10, receive.LabTestRequestId);
        Assert.Equal(3, receive.DispositionId);
        Assert.Equal(7, receive.ReceivedByUserId);
        Assert.Equal("ok", receive.Remarks);
        Assert.Equal("Accepted", receive.Status);
    }

    [Fact]
    public async Task Handle_fails_for_an_unknown_test_request()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var handler = new AcceptLabReceiveRequestCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new AcceptLabReceiveRequestCommand(99, null, null, null), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.LabReceiveRequestsRepo.All);
    }
}
