using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;

namespace Ultramaverick.Warehouse.Application.Commands.Inventory;

public sealed class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Result>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly InventoryLedger _ledger;

    public AdjustStockCommandHandler(IWarehouseUnitOfWork uow, InventoryLedger ledger)
    {
        _uow = uow;
        _ledger = ledger;
    }

    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        try
        {
            await _ledger.PostAsync(
                request.WarehouseId, request.ItemId, request.UomId, request.LotNo, request.Delta,
                MovementSource.Adjustment, sourceDocumentId: null, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
