using Ultramaverick.SharedKernel;
using Ultramaverick.Warehouse.Domain.Events;

namespace Ultramaverick.Warehouse.Domain.Entities;

public sealed class Warehouse : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int? WarehouseTypeId { get; private set; }   // scalar → Catalog (no lookup table this wave)
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(
        string code, string name, int? warehouseTypeId, string? address, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var warehouse = new Warehouse
        {
            Code = code,
            Name = name,
            WarehouseTypeId = warehouseTypeId,
            Address = address,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        warehouse.RaiseDomainEvent(new WarehouseChanged(WarehouseChangeTypes.Created, DateTime.UtcNow));
        return warehouse;
    }

    public void Update(string name, int? warehouseTypeId, string? address, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        WarehouseTypeId = warehouseTypeId;
        Address = address;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;

        RaiseDomainEvent(new WarehouseChanged(WarehouseChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool isActive, int modifiedByUserId)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;

        RaiseDomainEvent(new WarehouseChanged(
            isActive ? WarehouseChangeTypes.Activated : WarehouseChangeTypes.Deactivated, DateTime.UtcNow));
    }
}
