using System.Security.Cryptography;
using System.Text;

namespace backend.Security;

public static class PasswordHasher
{
    public static string ComputeSha256Hex(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
