using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Services;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ReferenceDataService(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<LookupDto>> ListAsync(
        string entity, int page, int pageSize, string? search, bool? isActive, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;
        var skip = (page - 1) * pageSize;

        switch (entity)
        {
            case "itemCategory":
                {
                    var rows = await _uow.ItemCategories.ListAsync(search, isActive, skip, pageSize, ct);
                    var total = await _uow.ItemCategories.CountAsync(search, isActive, ct);
                    return Page(rows.Select(x => new LookupDto(x.Id, null, x.Name, x.IsActive)), page, pageSize, total);
                }
            case "lotCategory":
                {
                    var rows = await _uow.LotCategories.ListAsync(search, isActive, skip, pageSize, ct);
                    var total = await _uow.LotCategories.CountAsync(search, isActive, ct);
                    return Page(rows.Select(x => new LookupDto(x.Id, x.Code, x.Name, x.IsActive)), page, pageSize, total);
                }
            case "transactionType":
                {
                    var rows = await _uow.TransactionTypes.ListAsync(search, isActive, skip, pageSize, ct);
                    var total = await _uow.TransactionTypes.CountAsync(search, isActive, ct);
                    return Page(rows.Select(x => new LookupDto(x.Id, x.Code, x.Name, x.IsActive)), page, pageSize, total);
                }
            case "sampleType":
                {
                    var rows = await _uow.SampleTypes.ListAsync(search, isActive, skip, pageSize, ct);
                    var total = await _uow.SampleTypes.CountAsync(search, isActive, ct);
                    return Page(rows.Select(x => new LookupDto(x.Id, x.Code, x.Name, x.IsActive)), page, pageSize, total);
                }
            case "farm":
                {
                    var rows = await _uow.Farms.ListAsync(search, isActive, skip, pageSize, ct);
                    var total = await _uow.Farms.CountAsync(search, isActive, ct);
                    return Page(rows.Select(x => new LookupDto(x.Id, x.FarmCode, x.FarmName, x.IsActive)), page, pageSize, total);
                }
            default:
                return new PagedResult<LookupDto>(Array.Empty<LookupDto>(), page, pageSize, 0);
        }
    }

    public async Task<LookupDto?> GetAsync(string entity, int id, CancellationToken ct)
    {
        switch (entity)
        {
            case "itemCategory":
                {
                    var x = await _uow.ItemCategories.GetByIdAsync(id, ct);
                    return x is null ? null : new LookupDto(x.Id, null, x.Name, x.IsActive);
                }
            case "lotCategory":
                {
                    var x = await _uow.LotCategories.GetByIdAsync(id, ct);
                    return x is null ? null : new LookupDto(x.Id, x.Code, x.Name, x.IsActive);
                }
            case "transactionType":
                {
                    var x = await _uow.TransactionTypes.GetByIdAsync(id, ct);
                    return x is null ? null : new LookupDto(x.Id, x.Code, x.Name, x.IsActive);
                }
            case "sampleType":
                {
                    var x = await _uow.SampleTypes.GetByIdAsync(id, ct);
                    return x is null ? null : new LookupDto(x.Id, x.Code, x.Name, x.IsActive);
                }
            case "farm":
                {
                    var x = await _uow.Farms.GetByIdAsync(id, ct);
                    return x is null ? null : new LookupDto(x.Id, x.FarmCode, x.FarmName, x.IsActive);
                }
            default:
                return null;
        }
    }

    public async Task<Result<int>> CreateAsync(string entity, string name, string? code, CancellationToken ct)
    {
        name = (name ?? string.Empty).Trim();
        code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();

        if (name.Length == 0) return Result<int>.Failure("A name is required.");
        var actor = _currentUser.UserId;

        switch (entity)
        {
            case "itemCategory":
                {
                    var x = ItemCategory.Create(name, actor);
                    await _uow.ItemCategories.AddAsync(x, ct);
                    await _uow.SaveChangesAsync(ct);
                    return Result<int>.Success(x.Id);
                }
            case "lotCategory":
                {
                    var x = LotCategory.Create(name, actor, code);
                    await _uow.LotCategories.AddAsync(x, ct);
                    await _uow.SaveChangesAsync(ct);
                    return Result<int>.Success(x.Id);
                }
            case "transactionType":
                {
                    var x = TransactionType.Create(name, actor, code);
                    await _uow.TransactionTypes.AddAsync(x, ct);
                    await _uow.SaveChangesAsync(ct);
                    return Result<int>.Success(x.Id);
                }
            case "sampleType":
                {
                    var x = SampleType.Create(name, actor, code);
                    await _uow.SampleTypes.AddAsync(x, ct);
                    await _uow.SaveChangesAsync(ct);
                    return Result<int>.Success(x.Id);
                }
            case "farm":
                {
                    if (code is null) return Result<int>.Failure("A farm code is required.");
                    var x = Farm.Create(code, name, actor);
                    await _uow.Farms.AddAsync(x, ct);
                    await _uow.SaveChangesAsync(ct);
                    return Result<int>.Success(x.Id);
                }
            default:
                return Result<int>.Failure("Unknown reference entity.");
        }
    }

    public async Task<Result> UpdateAsync(string entity, int id, string name, string? code, CancellationToken ct)
    {
        name = (name ?? string.Empty).Trim();
        code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (name.Length == 0) return Result.Failure("A name is required.");
        var actor = _currentUser.UserId ?? 0;

        switch (entity)
        {
            case "itemCategory":
                {
                    var x = await _uow.ItemCategories.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.Rename(name, actor);
                    break;
                }
            case "lotCategory":
                {
                    var x = await _uow.LotCategories.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.Rename(name, code, actor);
                    break;
                }
            case "transactionType":
                {
                    var x = await _uow.TransactionTypes.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.Rename(name, code, actor);
                    break;
                }
            case "sampleType":
                {
                    var x = await _uow.SampleTypes.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.Rename(name, code, actor);
                    break;
                }
            case "farm":
                {
                    if (code is null) return Result.Failure("A farm code is required.");
                    var x = await _uow.Farms.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.Update(code, name, actor);
                    break;
                }
            default:
                return Result.Failure("Unknown reference entity.");
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(string entity, int id, bool isActive, CancellationToken ct)
    {
        var actor = _currentUser.UserId ?? 0;

        switch (entity)
        {
            case "itemCategory":
                {
                    var x = await _uow.ItemCategories.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.SetActive(isActive, actor);
                    break;
                }
            case "lotCategory":
                {
                    var x = await _uow.LotCategories.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.SetActive(isActive, actor);
                    break;
                }
            case "transactionType":
                {
                    var x = await _uow.TransactionTypes.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.SetActive(isActive, actor);
                    break;
                }
            case "sampleType":
                {
                    var x = await _uow.SampleTypes.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.SetActive(isActive, actor);
                    break;
                }
            case "farm":
                {
                    var x = await _uow.Farms.GetByIdAsync(id, ct);
                    if (x is null) return Result.Failure("Not found.");
                    x.SetActive(isActive, actor);
                    break;
                }
            default:
                return Result.Failure("Unknown reference entity.");
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static PagedResult<LookupDto> Page(
        IEnumerable<LookupDto> rows, int page, int pageSize, int total)
        => new(rows.ToList(), page, pageSize, total);
}
