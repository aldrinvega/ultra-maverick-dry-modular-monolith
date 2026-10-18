using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using Ultramaverick.Warehouse.Application.Services;
using Ultramaverick.Warehouse.Domain;
using Ultramaverick.Warehouse.Domain.Entities;

namespace Ultramaverick.Warehouse.Application.Commands.WarehouseReceipts;

public sealed class CreateWarehouseReceiptCommandHandler
    : IRequestHandler<CreateWarehouseReceiptCommand, Result<int>>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly InventoryLedger _ledger;
    private readonly ICurrentUser _currentUser;

    public CreateWarehouseReceiptCommandHandler(
        IWarehouseUnitOfWork uow, InventoryLedger ledger, ICurrentUser currentUser)
    {
        _uow = uow;
        _ledger = ledger;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateWarehouseReceiptCommand request, CancellationToken ct)
    {
        // Manual receipt: no originating QC receipt, so the ledger posts a MiscReceipt movement
        // with no source document id (see brief "Hidden issue").
        var receipt = WarehouseReceipt.Create(
            qcReceiptId: null,
            warehouseId: request.WarehouseId,
            itemId: request.ItemId,
            uomId: request.UomId,
            lotNo: request.LotNo,
            quantity: request.Quantity,
            manufacturingDate: request.ManufacturingDate,
            expiryDate: request.ExpiryDate,
            receivedBy: _currentUser.UserId);

        await _uow.Receipts.AddAsync(receipt, ct);

        await _ledger.PostAsync(
            request.WarehouseId, request.ItemId, request.UomId, request.LotNo, request.Quantity,
            MovementSource.MiscReceipt, sourceDocumentId: null, ct);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(receipt.Id);
    }
}
