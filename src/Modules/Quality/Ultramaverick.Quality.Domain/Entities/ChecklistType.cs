using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

/// <summary>
/// Template header for a checklist (§7.1). Derived from the §7.1 table note
/// "template header, ProductTypeId (scalar)": name identifies the template and
/// ProductTypeId scopes it to a Catalog product type.
/// </summary>
public sealed class ChecklistType : Entity
{
    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public string Name { get; private set; } = null!;
    public int? ProductTypeId { get; private set; }   // scalar → Catalog.ProductTypes
    public bool IsActive { get; private set; }

    private ChecklistType() { }

    public static ChecklistType Create(string name, int? productTypeId)
    {
        name = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ChecklistType { Name = name, ProductTypeId = productTypeId, IsActive = true };
    }
}
