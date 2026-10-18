using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;
using Ultramaverick.Laboratory.Application.Commands.LabTestRequests;
using Ultramaverick.Laboratory.Application.Commands.LabTestResults;

namespace Ultramaverick.Laboratory.IntegrationTests;

/// <summary>
/// Drives the real use cases through MediatR against SQL Server: create a test request,
/// accept it, record its result, then assert the result row is linked by a real foreign key
/// to the receive request (the corrected relationships from §8).
/// </summary>
public class LabReceiveRequestResultLinkTests : IClassFixture<DatabaseFixture>
{
    private const string ResultForeignKey =
        "FK_LabTestResults_LabReceiveRequests_LabReceiveRequestId";

    private readonly DatabaseFixture _fixture;

    public LabReceiveRequestResultLinkTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Result_rows_point_at_the_receive_request_through_a_foreign_key()
    {
        using var scope = _fixture.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var created = await mediator.Send(new CreateLabTestRequestCommand(
            WarehouseReceiptId: null,
            BatchNo: TestData.Code("LOT"),
            Quantity: 5m,
            TestType: "Micro",
            DateRequested: null,
            DateNeeded: null,
            AnalysisIds: new[] { 1 },
            ParameterIds: null,
            ProductConditionIds: null,
            SampleTypeIds: null,
            SwabTypeIds: null));

        Assert.True(created.Succeeded);

        var accepted = await mediator.Send(new AcceptLabReceiveRequestCommand(
            LabTestRequestId: created.Value,
            DispositionId: null,
            DateReceived: DateTime.UtcNow,
            Remarks: "received"));

        Assert.True(accepted.Succeeded);

        var recorded = await mediator.Send(new RecordLabResultCommand(
            LabReceiveRequestId: accepted.Value,
            AnalyzedAt: DateTime.UtcNow,
            Result: "Passed"));

        Assert.True(recorded.Succeeded);

        await using var context = _fixture.CreateContext();

        var result = await context.LabTestResults
            .SingleAsync(x => x.LabReceiveRequestId == accepted.Value);

        Assert.Equal("Passed", result.Result);
        Assert.Equal("ForQaApproval", result.Status);

        var foreignKeyCount = await context.Database
            .SqlQueryRaw<int>(
                $"SELECT COUNT(*) AS Value FROM sys.foreign_keys WHERE name = '{ResultForeignKey}'")
            .SingleAsync();

        Assert.Equal(1, foreignKeyCount);
    }
}
