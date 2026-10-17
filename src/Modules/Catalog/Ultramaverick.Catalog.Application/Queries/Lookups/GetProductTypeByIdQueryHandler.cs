using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed class GetProductTypeByIdQueryHandler : IRequestHandler<GetProductTypeByIdQuery, Result<LookupDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetProductTypeByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<LookupDto>> Handle(GetProductTypeByIdQuery request, CancellationToken ct)
    {
        var entity = await _uow.ProductTypes.GetByIdAsync(request.ProductTypeId, ct);
        if (entity is null) return Result<LookupDto>.Failure("Product type not found.");

        return Result<LookupDto>.Success(new LookupDto(entity.Id, entity.Code, entity.Name, entity.IsActive));
    }
}