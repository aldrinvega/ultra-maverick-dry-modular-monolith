using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.LotNames;

public sealed class CreateLotNameCommandHandler : IRequestHandler<CreateLotNameCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateLotNameCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateLotNameCommand request, CancellationToken ct)
    {
        var code = (request.Code ?? string.Empty).Trim();

        if (await _uow.LotNames.GetByCodeAsync(code, ct) is not null)
            return Result<int>.Failure($"Lot name code '{code}' already exists.");
        if (await _uow.LotCategories.GetByIdAsync(request.LotCategoryId, ct) is null)
            return Result<int>.Failure("Lot category does not exist.");

        var lotName = LotName.Create(code, request.LotCategoryId, request.SectionName, _currentUser.UserId);

        await _uow.LotNames.AddAsync(lotName, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(lotName.Id);
    }
}