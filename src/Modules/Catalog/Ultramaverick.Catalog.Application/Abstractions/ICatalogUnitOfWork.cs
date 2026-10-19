using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Abstractions;

/// <summary>Aggregates every Catalog repository behind one transaction boundary.</summary>
public interface ICatalogUnitOfWork
{
    IItemRepository Items { get; }
    IUomRepository Uoms { get; }
    ISupplierRepository Suppliers { get; }
    IFarmRepository Farms { get; }
    ICustomerRepository Customers { get; }
    ILotNameRepository LotNames { get; }
    IReasonRepository Reasons { get; }
    IFormulaRepository Formulas { get; }

    ILookupRepository<ItemCategory> ItemCategories { get; }
    ILookupRepository<LotCategory> LotCategories { get; }
    ILookupRepository<Location> Locations { get; }
    ILookupRepository<AccountTitle> AccountTitles { get; }
    ILookupRepository<ProductType> ProductTypes { get; }
    ILookupRepository<TransactionType> TransactionTypes { get; }
    ILookupRepository<SampleType> SampleTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}