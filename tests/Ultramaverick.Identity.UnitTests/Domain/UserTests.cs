using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.Events;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.UnitTests.Domain;

public class UserTests
{
    private static PasswordHash Hash() => PasswordHash.FromEncoded("PBKDF2;SHA256;210000;AAAA;BBBB");

    [Fact]
    public void Create_trims_names_and_raises_created()
    {
        var user = User.Create("  Jane Doe  ", "  jdoe  ", Hash(), 1, 1, null);

        Assert.Equal("Jane Doe", user.FullName);
        Assert.Equal("jdoe", user.UserName);
        Assert.True(user.IsActive);
        Assert.Single(user.DomainEvents);

        var raised = Assert.IsType<UserChanged>(user.DomainEvents.First());
        Assert.Equal(UserChangeTypes.Created, raised.ChangeType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_full_name(string fullName)
        => Assert.Throws<ArgumentException>(() => User.Create(fullName, "jdoe", Hash(), 1, 1, null));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_user_name(string userName)
        => Assert.Throws<ArgumentException>(() => User.Create("Jane", userName, Hash(), 1, 1, null));

    [Fact]
    public void Deactivate_is_idempotent_and_raises_once()
    {
        var user = User.Create("Jane", "jdoe", Hash(), 1, 1, null);
        user.ClearDomainEvents();

        user.Deactivate(9);
        user.Deactivate(9);

        Assert.False(user.IsActive);
        Assert.Single(user.DomainEvents);
        Assert.Equal(UserChangeTypes.Deactivated, ((UserChanged)user.DomainEvents.First()).ChangeType);
    }

    [Fact]
    public void Activate_raises_only_when_inactive()
    {
        var user = User.Create("Jane", "jdoe", Hash(), 1, 1, null);
        user.Deactivate(9);
        user.ClearDomainEvents();

        user.Activate(9);
        user.Activate(9);

        Assert.True(user.IsActive);
        Assert.Single(user.DomainEvents);
    }

    [Fact]
    public void ChangePassword_raises_password_changed()
    {
        var user = User.Create("Jane", "jdoe", Hash(), 1, 1, null);
        user.ClearDomainEvents();

        user.ChangePassword(PasswordHash.FromEncoded("PBKDF2;SHA256;210000;CCCC;DDDD"), 9);

        Assert.Single(user.DomainEvents);
        Assert.Equal(UserChangeTypes.PasswordChanged, ((UserChanged)user.DomainEvents.First()).ChangeType);
    }

    [Fact]
    public void ChangePassword_rejects_blank_hash()
    {
        var user = User.Create("Jane", "jdoe", Hash(), 1, 1, null);
        Assert.Throws<ArgumentException>(
            () => user.ChangePassword(PasswordHash.FromEncoded("   "), 9));
    }

    [Fact]
    public void UpdateProfile_trims_and_raises_updated()
    {
        var user = User.Create("Jane", "jdoe", Hash(), 1, 1, null);
        user.ClearDomainEvents();

        user.UpdateProfile("  Jane R.  ", "  jane.r  ", 2, 2, 9);

        Assert.Equal("Jane R.", user.FullName);
        Assert.Equal("jane.r", user.UserName);
        Assert.Equal(UserChangeTypes.Updated, ((UserChanged)user.DomainEvents.First()).ChangeType);
    }
}
