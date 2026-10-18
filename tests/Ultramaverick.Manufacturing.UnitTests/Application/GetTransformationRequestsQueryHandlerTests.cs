using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.Manufacturing.Application.Queries.TransformationRequests;

namespace Ultramaverick.Manufacturing.UnitTests.Application;

internal sealed class FakeTransformationRequestReader : ITransformationRequestReader
{
    public PagedResult<TransformationRequestSummaryDto> Page { get; set; } =
        new(Array.Empty<TransformationRequestSummaryDto>(), 1, 25, 0);

    public TransformationRequestDetailDto? Detail { get; set; }
    public int? LastPage { get; private set; }
    public int? LastPageSize { get; private set; }

    public Task<PagedResult<TransformationRequestSummaryDto>> GetPagedAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        LastPage = page;
        LastPageSize = pageSize;
        return Task.FromResult(Page);
    }

    public Task<TransformationRequestDetailDto?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(Detail);
}

public class GetTransformationRequestsQueryHandlerTests
{
    [Theory]
    [InlineData(0, 0, 1, 25)]
    [InlineData(-4, 900, 1, 100)]
    public async Task Listing_clamps_the_page_window(
        int requestedPage, int requestedSize, int expectedPage, int expectedSize)
    {
        var reader = new FakeTransformationRequestReader();
        var handler = new GetTransformationRequestsQueryHandler(reader);

        var result = await handler.Handle(
            new GetTransformationRequestsQuery(null, requestedPage, requestedSize), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedPage, reader.LastPage);
        Assert.Equal(expectedSize, reader.LastPageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task An_invalid_id_is_a_failure(int id)
    {
        var handler = new GetTransformationRequestByIdQueryHandler(new FakeTransformationRequestReader());

        var result = await handler.Handle(new GetTransformationRequestByIdQuery(id), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task A_missing_request_is_a_failure()
    {
        var handler = new GetTransformationRequestByIdQueryHandler(new FakeTransformationRequestReader());

        var result = await handler.Handle(new GetTransformationRequestByIdQuery(5), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Transformation request not found.", result.Error);
    }
}
