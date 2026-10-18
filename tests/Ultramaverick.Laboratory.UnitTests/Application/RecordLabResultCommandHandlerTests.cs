using Ultramaverick.Laboratory.Application.Commands.LabTestResults;
using Ultramaverick.Laboratory.Contracts;
using Ultramaverick.Laboratory.Domain.Entities;
using Ultramaverick.Laboratory.UnitTests.Fakes;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class RecordLabResultCommandHandlerTests
{
    [Fact]
    public async Task Handle_records_a_result_linked_to_the_receive_request()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var testRequest = LabTestRequest.Create(11, "LOT-1", 5m, "Micro", null, null, 7);
        uow.LabTestRequestsRepo.SeedWithId(10, testRequest);
        uow.LabReceiveRequestsRepo.SeedWithId(20, LabReceiveRequest.Create(10, null, null, 7, null));
        var handler = new RecordLabResultCommandHandler(
            uow, new FakeCurrentUser { UserId = 9 });

        var analyzedAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        var result = await handler.Handle(
            new RecordLabResultCommand(20, analyzedAt, "Passed"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);

        var saved = Assert.Single(uow.LabTestResultsRepo.All);
        Assert.Equal(20, saved.LabReceiveRequestId);
        Assert.Equal(analyzedAt, saved.DateAnalyzed);
        Assert.Equal("Passed", saved.Result);
        Assert.Equal("ForQaApproval", saved.Status);
        Assert.Equal(9, saved.AnalyzedByUserId);

        var raised = Assert.IsType<LabResultRecorded>(Assert.Single(saved.DomainEvents));
        Assert.Equal(20, raised.LabReceiveRequestId);
        Assert.Equal(10, raised.LabTestRequestId);
        Assert.Equal(11, raised.WarehouseReceiptId);
        Assert.Equal("LOT-1", raised.BatchNo);
    }

    [Fact]
    public async Task Handle_fails_for_an_unknown_receive_request()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        var handler = new RecordLabResultCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new RecordLabResultCommand(99, DateTime.UtcNow, "Passed"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.LabTestResultsRepo.All);
    }

    [Fact]
    public async Task Handle_fails_when_the_test_request_is_missing()
    {
        var uow = new FakeLaboratoryUnitOfWork();
        uow.LabReceiveRequestsRepo.SeedWithId(
            20, LabReceiveRequest.Create(99, null, null, null, null));
        var handler = new RecordLabResultCommandHandler(uow, new FakeCurrentUser());

        var result = await handler.Handle(
            new RecordLabResultCommand(20, DateTime.UtcNow, "Passed"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(uow.LabTestResultsRepo.All);
    }
}
