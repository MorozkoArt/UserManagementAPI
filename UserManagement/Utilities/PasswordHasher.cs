namespace UserManagement.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public static string HashPassword(string password)
    {
        using var algorithm = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password, 
            SaltSize, 
            Iterations, 
            System.Security.Cryptography.HashAlgorithmName.SHA256);

            var salt = algorithm.Salt;
            var hash = algorithm.GetBytes(HashSize);

            var combined = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);

            return Convert.ToBase64String(combined);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var combined = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];

        Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(combined, SaltSize, hash, 0, HashSize);

        using var algorithm = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA256);

        var newHash = algorithm.GetBytes(HashSize);
        return newHash.SequenceEqual(hash);
    }
}