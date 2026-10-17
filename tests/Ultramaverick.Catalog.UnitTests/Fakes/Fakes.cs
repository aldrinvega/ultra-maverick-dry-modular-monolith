using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.UnitTests.Fakes;

internal sealed class FakeItemRepository : IItemRepository
{
    private readonly List<Item> _items = new();

    public IReadOnlyList<Item> All => _items;

    public Task<Item?> GetByIdAsync(int id, CancellationToken ct)
        => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

    public Task<Item?> GetByCodeAsync(string itemCode, CancellationToken ct)
        => Task.FromResult(_items.FirstOrDefault(i => i.ItemCode == itemCode));

    public Task<IReadOnlyList<Item>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(Item item, CancellationToken ct)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUomRepository : IUomRepository
{
    private readonly List<Uom> _uoms = new();

    public IReadOnlyList<Uom> All => _uoms;

    public void Seed(Uom uom) => _uoms.Add(uom);

    public Task<Uom?> GetByIdAsync(int id, CancellationToken ct)
        // Entities created via Uom.Create have Id 0 until a database assigns one, so fall
        // back to the single seeded row when no id matches.
        => Task.FromResult(_uoms.FirstOrDefault(u => u.Id == id) ?? _uoms.FirstOrDefault());

    public Task<Uom?> GetByCodeAsync(string code, CancellationToken ct)
        => Task.FromResult(_uoms.FirstOrDefault(u => u.Code == code));

    public Task<IReadOnlyList<Uom>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(Uom uom, CancellationToken ct)
    {
        _uoms.Add(uom);
        return Task.CompletedTask;
    }
}

internal sealed class FakeLookupRepository<T> : ILookupRepository<T> where T : class
{
    private readonly List<T> _items = new();
    private readonly Func<T, int> _id;
    private readonly Func<T, string> _name;

    public FakeLookupRepository(Func<T, int> id, Func<T, string> name)
    {
        _id = id;
        _name = name;
    }

    public void Seed(T entity) => _items.Add(entity);

    public Task<T?> GetByIdAsync(int id, CancellationToken ct)
        // Seeded entities carry Id 0 until persisted, so fall back to the single seeded row.
        => Task.FromResult(_items.FirstOrDefault(e => _id(e) == id) ?? _items.FirstOrDefault());

    public Task<T?> GetByNameAsync(string name, CancellationToken ct)
        => Task.FromResult(_items.FirstOrDefault(e => _name(e) == name));

    public Task<T?> GetByCodeAsync(string code, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<T>> ListAsync(string? search, bool? isActive, int skip, int take, CancellationToken ct)
        => throw new NotSupportedException();

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken ct)
        => throw new NotSupportedException();

    public Task AddAsync(T entity, CancellationToken ct)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public int? UserId { get; set; } = 1;
    public string? UserName { get; set; } = "tester";
    public bool IsAuthenticated { get; set; } = true;
}

internal sealed class FakeCatalogUnitOfWork : ICatalogUnitOfWork
{
    public FakeItemRepository ItemsRepo { get; } = new();
    public FakeUomRepository UomsRepo { get; } = new();
    public FakeLookupRepository<ItemCategory> ItemCategoriesRepo { get; } =
        new(category => category.Id, category => category.Name);

    public int SaveCount { get; private set; }

    public IItemRepository Items => ItemsRepo;
    public IUomRepository Uoms => UomsRepo;
    public ILookupRepository<ItemCategory> ItemCategories => ItemCategoriesRepo;
    public ILookupRepository<LotCategory> LotCategories => throw new NotSupportedException();
    public ILookupRepository<Location> Locations => throw new NotSupportedException();
    public ILookupRepository<AccountTitle> AccountTitles => throw new NotSupportedException();
    public ILookupRepository<ProductType> ProductTypes => throw new NotSupportedException();
    public ISupplierRepository Suppliers => throw new NotSupportedException();
    public IFarmRepository Farms => throw new NotSupportedException();
    public ICustomerRepository Customers => throw new NotSupportedException();
    public ILotNameRepository LotNames => throw new NotSupportedException();
    public IReasonRepository Reasons => throw new NotSupportedException();
    public IFormulaRepository Formulas => throw new NotSupportedException();

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(SaveCount);
    }
}