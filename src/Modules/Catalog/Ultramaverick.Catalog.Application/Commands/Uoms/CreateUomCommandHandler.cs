using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Uoms;

public sealed class CreateUomCommandHandler : IRequestHandler<CreateUomCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateUomCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateUomCommand request, CancellationToken ct)
    {
        var code = (request.Code ?? string.Empty).Trim();

        if (await _uow.Uoms.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"UOM code '{code}' already exists.");

        var uom = Uom.Create(code, request.Description, _currentUser.UserId);

        await _uow.Uoms.AddAsync(uom, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(uom.Id);
    }
}