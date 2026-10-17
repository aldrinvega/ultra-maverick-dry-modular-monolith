using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed class AddFormulaRequirementCommandHandler : IRequestHandler<AddFormulaRequirementCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public AddFormulaRequirementCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AddFormulaRequirementCommand request, CancellationToken ct)
    {
        var formula = await _uow.Formulas.GetByIdAsync(request.FormulaId, ct);
        if (formula is null) return Result.Failure("Formula not found.");

        if (await _uow.Items.GetByIdAsync(request.RawMaterialId, ct) is null)
            return Result.Failure("Raw material item does not exist.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        try
        {
            formula.AddRequirement(request.RawMaterialId, request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        formula.Update(formula.Quantity, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}