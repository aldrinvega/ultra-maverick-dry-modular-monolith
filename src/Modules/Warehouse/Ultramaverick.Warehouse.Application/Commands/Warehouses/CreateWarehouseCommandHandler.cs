using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;
using Ultramaverick.Warehouse.Application.Models;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.Application.Commands.Warehouses;

public sealed class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<int>>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateWarehouseCommandHandler(IWarehouseUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        var code = (request.Code ?? string.Empty).Trim();
        if (await _uow.Warehouses.CodeExistsAsync(code, ct))
            return Result<int>.Failure($"Warehouse code '{code}' already exists.");

        var warehouse = WarehouseEntity.Create(
            code, request.Name, request.WarehouseTypeId, request.Address, _currentUser.UserId);

        await _uow.Warehouses.AddAsync(warehouse, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(warehouse.Id);
    }
}
