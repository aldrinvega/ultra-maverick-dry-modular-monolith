using MediatR;
using Ultramaverick.Procurement.Application.Abstractions;
using Ultramaverick.Procurement.Application.Models;
using Ultramaverick.Procurement.Domain.Entities;

namespace Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

public sealed class CreatePurchaseOrderCommandHandler
    : IRequestHandler<CreatePurchaseOrderCommand, Result<int>>
{
    private readonly IProcurementUnitOfWork _uow;
    private readonly ISupplierLookup _suppliers;   // query port into Catalog
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(
        IProcurementUnitOfWork uow, ISupplierLookup suppliers, ICurrentUser currentUser)
    {
        _uow = uow;
        _suppliers = suppliers;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        if (await _uow.PurchaseOrders.GetByNumberAsync(request.PoNumber, ct) is not null)
            return Result<int>.Failure($"PO '{request.PoNumber}' already exists.");

        if (!await _suppliers.ExistsAsync(request.SupplierId, ct))
            return Result<int>.Failure("Supplier does not exist.");

        var po = PurchaseOrder.Create(request.PoNumber, request.PrNumber, request.PoDate,
            request.PrDate, request.SupplierId, importBatchId: null, _currentUser.UserId);

        foreach (var line in request.Lines)
            po.AddLine(line.ItemId, line.UomId, line.Quantity, line.UnitPrice);

        await _uow.PurchaseOrders.AddAsync(po, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(po.Id);
    }
}
