using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Uoms;

public sealed class GetUomByIdQueryHandler : IRequestHandler<GetUomByIdQuery, Result<UomDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetUomByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<UomDto>> Handle(GetUomByIdQuery request, CancellationToken ct)
    {
        var uom = await _uow.Uoms.GetByIdAsync(request.UomId, ct);
        if (uom is null) return Result<UomDto>.Failure("UOM not found.");

        return Result<UomDto>.Success(new UomDto(uom.Id, uom.Code, uom.Description, uom.IsActive));
    }
}