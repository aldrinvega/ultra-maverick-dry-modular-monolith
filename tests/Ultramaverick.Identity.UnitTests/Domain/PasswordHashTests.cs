using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.UnitTests.Domain;

public class PasswordHashTests
{
    [Fact]
    public void FromEncoded_accepts_a_value()
    {
        var hash = PasswordHash.FromEncoded("abc");
        Assert.Equal("abc", hash.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromEncoded_rejects_blank(string value)
        => Assert.Throws<ArgumentException>(() => PasswordHash.FromEncoded(value));

    [Fact]
    public void FromEncoded_rejects_null()
        => Assert.Throws<ArgumentNullException>(() => PasswordHash.FromEncoded(null!));
}
