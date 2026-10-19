using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Persistence.Repositories;

public sealed class CatalogUnitOfWork : ICatalogUnitOfWork
{
    private readonly CatalogDbContext _context;

    public CatalogUnitOfWork(CatalogDbContext context)
    {
        _context = context;

        Items = new ItemRepository(context);
        Uoms = new UomRepository(context);
        Suppliers = new SupplierRepository(context);
        Farms = new FarmRepository(context);
        Customers = new CustomerRepository(context);
        LotNames = new LotNameRepository(context);
        Reasons = new ReasonRepository(context);
        Formulas = new FormulaRepository(context);

        ItemCategories = new LookupRepository<ItemCategory>(context);
        LotCategories = new LookupRepository<LotCategory>(context);
        Locations = new LookupRepository<Location>(context);
        AccountTitles = new LookupRepository<AccountTitle>(context);
        ProductTypes = new LookupRepository<ProductType>(context);
        TransactionTypes = new LookupRepository<TransactionType>(context);
        SampleTypes = new LookupRepository<SampleType>(context);
    }

    public IItemRepository Items { get; }
    public IUomRepository Uoms { get; }
    public ISupplierRepository Suppliers { get; }
    public IFarmRepository Farms { get; }
    public ICustomerRepository Customers { get; }
    public ILotNameRepository LotNames { get; }
    public IReasonRepository Reasons { get; }
    public IFormulaRepository Formulas { get; }

    public ILookupRepository<ItemCategory> ItemCategories { get; }
    public ILookupRepository<LotCategory> LotCategories { get; }
    public ILookupRepository<Location> Locations { get; }
    public ILookupRepository<AccountTitle> AccountTitles { get; }
    public ILookupRepository<ProductType> ProductTypes { get; }
    public ILookupRepository<TransactionType> TransactionTypes { get; }
    public ILookupRepository<SampleType> SampleTypes { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}