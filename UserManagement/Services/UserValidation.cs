using System.Text.RegularExpressions;

namespace UserManagement.Services;

public static partial class UserValidation
{
    public static void ValidateLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login) || !LoginRegex().IsMatch(login))
            throw new ArgumentException("Login can only contain Latin letters and numbers");
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || !PasswordRegex().IsMatch(password))
            throw new ArgumentException("Password can only contain Latin letters and numbers");
    }

    public static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !NameRegex().IsMatch(name))
            throw new ArgumentException("Name can only contain Russian and Latin letters");
    }

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex LoginRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"^[a-zA-Zа-яА-ЯёЁ\s]+$")]
    private static partial Regex NameRegex();
}