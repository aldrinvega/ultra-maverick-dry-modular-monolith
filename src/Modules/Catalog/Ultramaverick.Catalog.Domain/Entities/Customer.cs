using Ultramaverick.Catalog.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Catalog.Domain.Entities;

public sealed class Customer : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string CustomerCode { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public int? FarmId { get; private set; }
    public string? CompanyCode { get; private set; }
    public string? CompanyName { get; private set; }
    public int? AccountTitleId { get; private set; }
    public int? LocationId { get; private set; }
    public int? DepartmentId { get; private set; }
    public string? MobileNumber { get; private set; }
    public string? LeadMan { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedByUserId { get; private set; }

    private Customer() { }

    public static Customer Create(
        string code, string name, int? farmId, string? companyCode, string? companyName,
        int? accountTitleId, int? locationId, int? departmentId,
        string? mobileNumber, string? leadMan, string? address, int? createdByUserId)
    {
        code = (code ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var customer = new Customer
        {
            CustomerCode = code,
            CustomerName = name,
            FarmId = farmId,
            CompanyCode = Clean(companyCode),
            CompanyName = Clean(companyName),
            AccountTitleId = accountTitleId,
            LocationId = locationId,
            DepartmentId = departmentId,
            MobileNumber = Clean(mobileNumber),
            LeadMan = Clean(leadMan),
            Address = Clean(address),
            IsActive = true,
            CreatedByUserId = createdByUserId
        };
        customer.RaiseDomainEvent(new CustomerChanged(CustomerChangeTypes.Created, DateTime.UtcNow));
        return customer;
    }

    public void Update(
        string name, int? farmId, string? companyCode, string? companyName,
        int? accountTitleId, int? locationId, int? departmentId,
        string? mobileNumber, string? leadMan, string? address, int modifiedByUserId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        CustomerName = name;
        FarmId = farmId;
        CompanyCode = Clean(companyCode);
        CompanyName = Clean(companyName);
        AccountTitleId = accountTitleId;
        LocationId = locationId;
        DepartmentId = departmentId;
        MobileNumber = Clean(mobileNumber);
        LeadMan = Clean(leadMan);
        Address = Clean(address);
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new CustomerChanged(CustomerChangeTypes.Updated, DateTime.UtcNow));
    }

    public void SetActive(bool active, int modifiedByUserId)
    {
        if (IsActive == active) return;

        IsActive = active;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedByUserId = modifiedByUserId;
        RaiseDomainEvent(new CustomerChanged(
            active ? CustomerChangeTypes.Activated : CustomerChangeTypes.Deactivated, DateTime.UtcNow));
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}