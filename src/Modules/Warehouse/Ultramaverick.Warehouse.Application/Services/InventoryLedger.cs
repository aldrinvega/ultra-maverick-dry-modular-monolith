using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Services;

/// <summary>
/// The single place on-hand is mutated: it records an append-only movement and folds the same
/// delta into the matching balance (created lazily on first movement). Callers commit with the
/// unit of work so the movement and the balance change land in one transaction.
/// </summary>
public sealed class InventoryLedger
{
    private readonly IWarehouseUnitOfWork _uow;

    public InventoryLedger(IWarehouseUnitOfWork uow) => _uow = uow;

    public async Task PostAsync(
        int warehouseId, int itemId, int uomId, string? lotNo, decimal delta,
        MovementSource source, int? sourceDocumentId, CancellationToken ct)
    {
        var balance = await _uow.Balances.GetForUpdateAsync(warehouseId, itemId, uomId, lotNo, ct);
        if (balance is null)
        {
            if (delta < 0)
                throw new InvalidOperationException(
                    $"No inventory balance exists for item {itemId} in warehouse {warehouseId}.");

            balance = InventoryBalance.Create(warehouseId, itemId, uomId, lotNo);
            await _uow.Balances.AddAsync(balance, ct);
        }

        balance.Apply(delta);

        await _uow.Movements.AddAsync(
            InventoryMovement.Create(warehouseId, itemId, uomId, lotNo, delta, source, sourceDocumentId), ct);
    }
}
