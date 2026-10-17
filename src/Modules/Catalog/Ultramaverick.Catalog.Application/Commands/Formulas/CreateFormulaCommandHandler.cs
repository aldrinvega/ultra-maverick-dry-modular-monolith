using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed class CreateFormulaCommandHandler : IRequestHandler<CreateFormulaCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateFormulaCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateFormulaCommand request, CancellationToken ct)
    {
        if (await _uow.Items.GetByIdAsync(request.ItemId, ct) is null)
            return Result<int>.Failure("Item does not exist.");
        if (await _uow.Formulas.GetByItemAndVersionAsync(request.ItemId, request.Version, ct) is not null)
            return Result<int>.Failure($"Formula version {request.Version} already exists for this item.");

        var formula = FormulaHeader.Create(request.ItemId, request.Version, request.Quantity, _currentUser.UserId);

        await _uow.Formulas.AddAsync(formula, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(formula.Id);
    }
}