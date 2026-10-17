using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Formulas;

public sealed class SetFormulaActiveCommandHandler : IRequestHandler<SetFormulaActiveCommand, Result>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SetFormulaActiveCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetFormulaActiveCommand request, CancellationToken ct)
    {
        var formula = await _uow.Formulas.GetByIdAsync(request.FormulaId, ct);
        if (formula is null) return Result.Failure("Formula not found.");

        var actorId = _currentUser.UserId
            ?? throw new InvalidOperationException("An authenticated user is required.");

        formula.SetActive(request.IsActive, actorId);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}