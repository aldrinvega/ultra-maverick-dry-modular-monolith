using Ultramaverick.Identity.Infrastructure.Security;

namespace Ultramaverick.Identity.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_then_verify_round_trips()
    {
        var encoded = _hasher.Hash("S3cret!pass");
        Assert.True(_hasher.Verify("S3cret!pass", encoded));
    }

    [Fact]
    public void Verify_is_false_for_a_wrong_password()
    {
        var encoded = _hasher.Hash("S3cret!pass");
        Assert.False(_hasher.Verify("wrong-password", encoded));
    }

    [Theory]
    [InlineData("not-a-hash")]
    [InlineData("PBKDF2;SHA256;210000;!!!;!!!")]
    [InlineData("PBKDF2;SHA256;;AAAA;BBBB")]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_is_false_for_a_malformed_hash(string encoded)
        => Assert.False(_hasher.Verify("S3cret!pass", encoded));

    [Fact]
    public void Verify_is_false_for_a_blank_password()
    {
        var encoded = _hasher.Hash("S3cret!pass");
        Assert.False(_hasher.Verify("", encoded));
    }

    [Fact]
    public void Two_hashes_of_the_same_password_differ()
    {
        Assert.NotEqual(_hasher.Hash("same"), _hasher.Hash("same"));
    }

    [Fact]
    public void Hash_rejects_a_blank_password()
        => Assert.Throws<ArgumentException>(() => _hasher.Hash("   "));
}
