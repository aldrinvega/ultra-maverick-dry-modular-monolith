using Ultramaverick.Warehouse.Domain.Events;
using WarehouseEntity = Ultramaverick.Warehouse.Domain.Entities.Warehouse;

namespace Ultramaverick.Warehouse.UnitTests.Domain;

public class WarehouseTests
{
    [Fact]
    public void Create_sets_active_and_raises_created()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", 3, "123 Depot Rd", 7);

        Assert.Equal("WH-1", warehouse.Code);
        Assert.Equal("Main", warehouse.Name);
        Assert.Equal(3, warehouse.WarehouseTypeId);
        Assert.Equal("123 Depot Rd", warehouse.Address);
        Assert.True(warehouse.IsActive);
        Assert.Equal(7, warehouse.CreatedByUserId);

        var raised = Assert.IsType<WarehouseChanged>(Assert.Single(warehouse.DomainEvents));
        Assert.Equal("Created", raised.ChangeType);
    }

    [Fact]
    public void Create_trims_code_and_name()
    {
        var warehouse = WarehouseEntity.Create("  WH-1  ", "  Main  ", null, null, null);

        Assert.Equal("WH-1", warehouse.Code);
        Assert.Equal("Main", warehouse.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_blank_code(string code)
        => Assert.Throws<ArgumentException>(() => WarehouseEntity.Create(code, "Main", null, null, null));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_blank_name(string name)
        => Assert.Throws<ArgumentException>(() => WarehouseEntity.Create("WH-1", name, null, null, null));

    [Fact]
    public void Update_changes_details_and_raises_updated()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", null, null, null);
        warehouse.ClearDomainEvents();

        warehouse.Update("Main Annex", 2, "456 Depot Rd", 9);

        Assert.Equal("Main Annex", warehouse.Name);
        Assert.Equal(2, warehouse.WarehouseTypeId);
        Assert.Equal("456 Depot Rd", warehouse.Address);
        Assert.Equal(9, warehouse.ModifiedByUserId);
        Assert.NotNull(warehouse.ModifiedAtUtc);

        var raised = Assert.IsType<WarehouseChanged>(Assert.Single(warehouse.DomainEvents));
        Assert.Equal("Updated", raised.ChangeType);
    }

    [Fact]
    public void SetActive_false_raises_deactivated()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", null, null, null);
        warehouse.ClearDomainEvents();

        warehouse.SetActive(false, 7);

        Assert.False(warehouse.IsActive);
        var raised = Assert.IsType<WarehouseChanged>(Assert.Single(warehouse.DomainEvents));
        Assert.Equal("Deactivated", raised.ChangeType);
    }

    [Fact]
    public void SetActive_true_raises_activated()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", null, null, null);
        warehouse.SetActive(false, 7);
        warehouse.ClearDomainEvents();

        warehouse.SetActive(true, 7);

        Assert.True(warehouse.IsActive);
        var raised = Assert.IsType<WarehouseChanged>(Assert.Single(warehouse.DomainEvents));
        Assert.Equal("Activated", raised.ChangeType);
    }

    [Fact]
    public void SetActive_to_the_same_value_is_a_no_op()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", null, null, null);
        warehouse.ClearDomainEvents();

        warehouse.SetActive(true, 7);

        Assert.Empty(warehouse.DomainEvents);
    }

    [Fact]
    public void WarehouseChanged_carries_no_id()
    {
        var warehouse = WarehouseEntity.Create("WH-1", "Main", null, null, null);

        var raised = Assert.IsType<WarehouseChanged>(Assert.Single(warehouse.DomainEvents));
        Assert.Null(raised.GetType().GetProperty("Id"));
    }
}
