using MediatR;
using Ultramaverick.Warehouse.Application.Abstractions;

namespace Ultramaverick.Warehouse.Application.Commands.Warehouses;

public sealed record SetWarehouseActiveCommand(int Id, bool IsActive) : IRequest<bool>;

public sealed class SetWarehouseActiveCommandHandler
    : IRequestHandler<SetWarehouseActiveCommand, bool>
{
    private readonly IWarehouseUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetWarehouseActiveCommandHandler(IWarehouseUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(SetWarehouseActiveCommand request, CancellationToken ct)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.Id, ct);
        if (warehouse is null)
            return false;

        if (_currentUser.UserId is not { } actorId)
            return false;

        warehouse.SetActive(request.IsActive, actorId);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
