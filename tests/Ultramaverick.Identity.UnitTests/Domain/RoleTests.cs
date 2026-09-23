using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.Events;

namespace Ultramaverick.Identity.UnitTests.Domain;

public class RoleTests
{
    [Fact]
    public void Create_trims_name_and_raises_created()
    {
        var role = Role.Create("  Supervisor  ", 1);

        Assert.Equal("Supervisor", role.Name);
        Assert.True(role.IsActive);
        Assert.Single(role.DomainEvents);
        Assert.Equal(RoleChangedTypes.Created,
            Assert.IsType<RoleChangedEvent>(role.DomainEvents.First()).ChangeType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_name(string name)
        => Assert.Throws<ArgumentException>(() => Role.Create(name, 1));

    [Fact]
    public void Deactivate_is_idempotent()
    {
        var role = Role.Create("Supervisor", 1);
        role.ClearDomainEvents();

        role.Deactivate(9);
        role.Deactivate(9);

        Assert.False(role.IsActive);
        Assert.Single(role.DomainEvents);
    }

    [Fact]
    public void Activate_raises_only_when_inactive()
    {
        var role = Role.Create("Supervisor", 1);
        role.Deactivate(9);
        role.ClearDomainEvents();

        role.Activate(9);
        role.Activate(9);

        Assert.True(role.IsActive);
        Assert.Single(role.DomainEvents);
    }

    [Fact]
    public void Rename_trims_and_raises_updated()
    {
        var role = Role.Create("Supervisor", 1);
        role.ClearDomainEvents();

        role.Rename("  Senior Supervisor  ", 9);

        Assert.Equal("Senior Supervisor", role.Name);
        Assert.Equal(RoleChangedTypes.Updated,
            Assert.IsType<RoleChangedEvent>(role.DomainEvents.First()).ChangeType);
    }
}
