using Ultramaverick.Catalog.Domain.Entities;
using Ultramaverick.Catalog.Domain.Events;

namespace Ultramaverick.Catalog.UnitTests.Domain;

public class UomTests
{
    [Fact]
    public void Create_trims_and_raises_created()
    {
        var uom = Uom.Create("  KG  ", "  Kilogram  ", 3);

        Assert.Equal("KG", uom.Code);
        Assert.Equal("Kilogram", uom.Description);
        Assert.True(uom.IsActive);

        var raised = Assert.IsType<UomChanged>(uom.DomainEvents.First());
        Assert.Equal(UomChangeTypes.Created, raised.ChangeType);
    }

    [Theory]
    [InlineData("", "Kilogram")]
    [InlineData("KG", "   ")]
    public void Create_rejects_blank_text(string code, string description)
        => Assert.Throws<ArgumentException>(() => Uom.Create(code, description, null));

    [Fact]
    public void SetActive_is_idempotent_and_raises_once()
    {
        var uom = Uom.Create("KG", "Kilogram", null);
        uom.ClearDomainEvents();

        uom.SetActive(false, 9);
        uom.SetActive(false, 9);

        Assert.False(uom.IsActive);
        Assert.Single(uom.DomainEvents);
        Assert.Equal(UomChangeTypes.Deactivated, ((UomChanged)uom.DomainEvents.First()).ChangeType);
    }
}