using System.Text;
using UserManagement.Models;
using System.Text.RegularExpressions;

namespace UserManagement.Services;

public static partial class UserValidation
{
    public static (bool IsValid, string? ErrorMessage) ValidateLogin(string login, Dictionary<string, User> _users)
    {
        if (login.StartsWith("_") || login.StartsWith("-") || login.StartsWith("."))
            return (false, "Login cannot start with characters _-.");

        if (login.EndsWith("_") || login.EndsWith("-") || login.EndsWith("."))
            return (false, "Login cannot end with characters _-.");

        if (_users.ContainsKey(login))
            return (false, "Login already exists");

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
    {
        var hasUpper = false;
        var hasLower = false;
        var hasDigit = false;
        var hasSpecial = false;

        foreach (var c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (SpecialChars.Contains(c)) hasSpecial = true;
        }

        var errors = new StringBuilder();
        if (!hasUpper) errors.AppendLine("- The password must contain at least one capital letter");
        if (!hasLower) errors.AppendLine("- The password must contain at least one lowercase letter");
        if (!hasDigit) errors.AppendLine("- The password must contain at least one digit");
        if (!hasSpecial) errors.AppendLine($"- The password must contain at least one special character: {SpecialChars}");

        if (errors.Length > 0)
            return (false, $"Password does not meet the requirements:\n{errors}");

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateName(string name)
    {
        if (name.StartsWith(" ") || name.StartsWith("-"))
            return (false, "A name cannot begin with a space or hyphen");

        if (name.EndsWith(" ") || name.EndsWith("-"))
            return (false, "A name cannot end with a space or a hyphen");

        if (name.Contains("  "))
            return (false, "The name cannot contain double spaces");

        return (true, null);
    }
    private const string SpecialChars = "!@#$%^&*()_+-=[]{};':\",./<>?\\|`~";

    public static (bool IsValid, string? ErrorMessage) ValidateBirthday(DateTime? birthday)
    {
        if (birthday > DateTime.Now)
            return (false, "Birthday cannot be in the future");
        return (true, null);
    }

}