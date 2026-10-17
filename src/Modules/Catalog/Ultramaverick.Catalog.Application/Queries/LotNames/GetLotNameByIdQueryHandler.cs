using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.LotNames;

public sealed class GetLotNameByIdQueryHandler : IRequestHandler<GetLotNameByIdQuery, Result<LotNameDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetLotNameByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<LotNameDto>> Handle(GetLotNameByIdQuery request, CancellationToken ct)
    {
        var lotName = await _uow.LotNames.GetByIdAsync(request.LotNameId, ct);
        if (lotName is null) return Result<LotNameDto>.Failure("Lot name not found.");

        var category = await _uow.LotCategories.GetByIdAsync(lotName.LotCategoryId, ct);

        return Result<LotNameDto>.Success(new LotNameDto(
            lotName.Id, lotName.Code, lotName.LotCategoryId,
            category?.Name ?? string.Empty, lotName.SectionName, lotName.IsActive));
    }
}