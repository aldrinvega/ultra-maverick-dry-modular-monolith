using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Application.Queries.InventoryPositions;
using Ultramaverick.Reporting.Application.Models;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.IntegrationTests;

public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void The_module_registers_exactly_its_five_event_handlers()
    {
        using var scope = _fixture.CreateScope();

        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>().ToList();

        Assert.Equal(5, handlers.Count);

        var eventTypes = handlers.Select(x => x.EventType).OrderBy(x => x).ToArray();
        Assert.Equal(
            new[] { "OrderCancelled", "OrderConfirmed", "OrderCreated", "QcReceiptCompleted", "StockReceived" },
            eventTypes);
    }

    [Fact]
    public void The_query_and_its_collaborators_resolve()
    {
        using var scope = _fixture.CreateScope();
        var provider = scope.ServiceProvider;

        Assert.NotNull(provider.GetRequiredService<IRequestHandler<GetInventoryPositionQuery, Result<IReadOnlyList<InventoryPositionDto>>>>());
        Assert.NotNull(provider.GetRequiredService<IReportingUnitOfWork>());
        Assert.NotNull(provider.GetRequiredService<IInventoryPositionReader>());
    }
}
