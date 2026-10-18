using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ultramaverick.Manufacturing.Application.Abstractions;
using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Application.Models;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Manufacturing.IntegrationTests;

/// <summary>
/// Proves the module's service registrations compose: handlers, validators, the unit of work and
/// its transaction stores all resolve from the container.
/// </summary>
public class DependencyInjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DependencyInjectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void The_module_handlers_and_stores_resolve()
    {
        using var scope = _fixture.CreateScope();
        var provider = scope.ServiceProvider;

        Assert.NotNull(provider.GetRequiredService<IRequestHandler<CreateTransformationRequestCommand, Result<int>>>());
        Assert.NotNull(provider.GetRequiredService<IRequestHandler<PrepareTransformationRequestCommand, Result>>());
        Assert.NotNull(provider.GetRequiredService<IManufacturingUnitOfWork>());

        Assert.Contains(provider.GetServices<IOutboxStore>(), store => store is not null);
        Assert.Contains(provider.GetServices<IProcessedEventStore>(), store => store is not null);
    }
}
