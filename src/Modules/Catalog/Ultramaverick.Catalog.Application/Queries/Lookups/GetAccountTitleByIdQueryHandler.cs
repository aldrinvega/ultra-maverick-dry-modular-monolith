using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Lookups;

public sealed class GetAccountTitleByIdQueryHandler : IRequestHandler<GetAccountTitleByIdQuery, Result<LookupDto>>
{
    private readonly ICatalogUnitOfWork _uow;

    public GetAccountTitleByIdQueryHandler(ICatalogUnitOfWork uow) => _uow = uow;

    public async Task<Result<LookupDto>> Handle(GetAccountTitleByIdQuery request, CancellationToken ct)
    {
        var entity = await _uow.AccountTitles.GetByIdAsync(request.AccountTitleId, ct);
        if (entity is null) return Result<LookupDto>.Failure("Account title not found.");

        return Result<LookupDto>.Success(new LookupDto(entity.Id, entity.Code, entity.Name, entity.IsActive));
    }
}