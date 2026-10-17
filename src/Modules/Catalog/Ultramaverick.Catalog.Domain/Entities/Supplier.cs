using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Supplier : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string SupplierCode { get; private set; } = null!;
    public string SupplierName { get; private set; } = null!;
    public string? SupplierAddress { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Supplier() { }

    public static Supplier Create(string code, string name, string? address, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var supplier = new Supplier
        {
            SupplierCode = code,
            SupplierName = name,
            SupplierAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        supplier.RaiseDomainEvent(new SupplierChanged(SupplierChangeTypes.Created, DateTime.UtcNow));
        return supplier;
    }

    public void Update(string code, string name, string? address, int modifiedByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        SupplierCode = code;
        SupplierName = name;
        SupplierAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new SupplierChanged(SupplierChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new SupplierChanged(
            active ? SupplierChangeTypes.Activated : SupplierChangeTypes.Deactivated, DateTime.UtcNow));
    }
}