using Microsoft.EntityFrameworkCore;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.IntegrationTests;

/// <summary>
/// Proves the five tag join tables persist and reload through their navigations, using the
/// same composite-key configuration as the legacy schema.
/// </summary>
public class JoinTableRoundTripTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public JoinTableRoundTripTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Tagged_join_rows_round_trip_with_their_ids()
    {
        int requestId;

        await using (var context = _fixture.CreateContext())
        {
            var request = LabTestRequest.Create(
                warehouseReceiptId: null, batchNo: TestData.Code("LOT"), quantity: 5m,
                testType: "Micro", dateRequested: null, dateNeeded: null, requestedByUserId: 1);

            request.TagAnalyses(new[] { 101, 102 });
            request.TagParameters(new[] { 201 });
            request.TagProductConditions(new[] { 301 });
            request.TagSampleTypes(new[] { 401 });
            request.TagSwabTypes(new[] { 501, 502 });

            context.LabTestRequests.Add(request);
            await context.SaveChangesAsync();

            requestId = request.Id;
        }

        await using (var context = _fixture.CreateContext())
        {
            var reloaded = await context.LabTestRequests
                .Include(x => x.Analyses)
                .Include(x => x.Parameters)
                .Include(x => x.ProductConditions)
                .Include(x => x.SampleTypes)
                .Include(x => x.SwabTypes)
                .SingleAsync(x => x.Id == requestId);

            Assert.Equal(new[] { 101, 102 }, reloaded.Analyses.Select(x => x.AnalysisId).OrderBy(x => x));
            Assert.Equal(new[] { 201 }, reloaded.Parameters.Select(x => x.ParameterId));
            Assert.Equal(new[] { 301 }, reloaded.ProductConditions.Select(x => x.ProductConditionId));
            Assert.Equal(new[] { 401 }, reloaded.SampleTypes.Select(x => x.SampleTypeId));
            Assert.Equal(new[] { 501, 502 }, reloaded.SwabTypes.Select(x => x.SwabTypeId).OrderBy(x => x));
        }
    }
}
