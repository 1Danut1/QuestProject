using backend.Security;

namespace backend.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void ComputeSha256Hex_IsDeterministic()
    {
        const string password = "my-secret";
        var first = PasswordHasher.ComputeSha256Hex(password);
        var second = PasswordHasher.ComputeSha256Hex(password);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputeSha256Hex_ProducesHexString()
    {
        var hash = PasswordHasher.ComputeSha256Hex("test");
        Assert.True(hash.Length > 0);
        Assert.All(hash, c => Assert.True(char.IsAsciiLetterOrDigit(c)));
    }

    [Fact]
    public void ComputeSha256Hex_DiffersForDifferentPasswords()
    {
        var a = PasswordHasher.ComputeSha256Hex("a");
        var b = PasswordHasher.ComputeSha256Hex("b");
        Assert.NotEqual(a, b);
    }
}
