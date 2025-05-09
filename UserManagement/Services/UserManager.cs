using UserManagement.Models;
using UserManagement.Models.Dtos;
using System.Text.RegularExpressions;

namespace UserManagement.Services;

public sealed partial class UserManager
{
    private readonly Dictionary<string, User> _users = [];
    private readonly ILogger<UserManager> _logger;

    public UserManager(ILogger<UserManager> logger)
    {
        _logger = logger;
        InitializeAdminUser();
    }

    private void InitializeAdminUser()
    {
        var admin = new User
        {
            Login = "Admin",
            Password = "Admin123",
            Name = "System Administrator",
            Gender = 1,
            Birthday = null,
            Admin = true,
            CreatedBy = "System",
            ModifiedBy = "System"
        };
        
        _users.Add(admin.Login, admin);
        _logger.LogInformation("Admin user initialized");
    }

    public IEnumerable<User> GetAllActiveUsers() => 
        _users.Values
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedOn);

    public IEnumerable<User> GetAllUsers() => 
        _users.Values.OrderBy(u => u.CreatedOn);

    public IEnumerable<User> GetUsersOlderThan(int age)
    {
        var cutoffDate = DateTime.Today.AddYears(-age);
        return _users.Values
            .Where(u => u.Birthday.HasValue && u.Birthday.Value.Date <= cutoffDate)
            .OrderBy(u => u.CreatedOn);
    }

    public User? GetByLogin(string login) => 
        _users.TryGetValue(login, out var user) ? user : null;

    public User? GetByCredentials(string login, string password) => 
        _users.TryGetValue(login, out var user) && user.Password == password && user.IsActive ? user : null;

    public bool LoginExists(string login) => 
        _users.ContainsKey(login);

    public User CreateUser(UserCreateDto dto, string createdBy)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(createdBy);

        ValidateLogin(dto.Login);
        ValidatePassword(dto.Password);
        ValidateName(dto.Name);

        if (LoginExists(dto.Login))
        {
            throw new ArgumentException("Login already exists");
        }

        var user = new User
        {
            Login = dto.Login,
            Password = dto.Password,
            Name = dto.Name,
            Gender = dto.Gender,
            Birthday = dto.Birthday,
            Admin = dto.Admin,
            CreatedBy = createdBy,
            ModifiedBy = createdBy
        };

        _users.Add(user.Login, user);
        _logger.LogInformation("User {Login} created by {CreatedBy}", user.Login, createdBy);
        return user;
    }

    public User UpdateUser(string login, UserUpdateDto dto, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        var user = GetByLogin(login) ?? throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrEmpty(dto.Name))
        {
            ValidateName(dto.Name);
            user.Name = dto.Name;
        }

        if (dto.Gender.HasValue)
        {
            user.Gender = dto.Gender.Value;
        }

        if (dto.Birthday.HasValue)
        {
            user.Birthday = dto.Birthday;
        }

        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;

        _logger.LogInformation("User {Login} updated by {ModifiedBy}", user.Login, modifiedBy);
        return user;
    }

    public User UpdatePassword(string login, string newPassword, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(newPassword);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        var user = GetByLogin(login) ?? throw new KeyNotFoundException("User not found");
        ValidatePassword(newPassword);
        
        user.Password = newPassword;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;

        _logger.LogInformation("Password for user {Login} updated by {ModifiedBy}", user.Login, modifiedBy);
        return user;
    }

    public User UpdateLogin(string oldLogin, string newLogin, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(newLogin);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        var user = GetByLogin(oldLogin) ?? throw new KeyNotFoundException("User not found");
        ValidateLogin(newLogin);
        
        if (LoginExists(newLogin))
        {
            throw new ArgumentException("New login already exists");
        }

        _users.Remove(oldLogin);
        user.Login = newLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        _users.Add(newLogin, user);

        _logger.LogInformation("Login for user {OldLogin} changed to {NewLogin} by {ModifiedBy}", 
            oldLogin, newLogin, modifiedBy);
        return user;
    }

    public void DeleteUser(string login, string revokedBy, bool softDelete = true)
    {
        ArgumentNullException.ThrowIfNull(revokedBy);

        var user = GetByLogin(login) ?? throw new KeyNotFoundException("User not found");

        if (softDelete)
        {
            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = revokedBy;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = revokedBy;
            _logger.LogInformation("User {Login} soft deleted by {RevokedBy}", user.Login, revokedBy);
        }
        else
        {
            _users.Remove(login);
            _logger.LogInformation("User {Login} permanently deleted by {RevokedBy}", login, revokedBy);
        }
    }

    public User RestoreUser(string login, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(modifiedBy);

        var user = GetByLogin(login) ?? throw new KeyNotFoundException("User not found");

        user.RevokedOn = null;
        user.RevokedBy = null;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;

        _logger.LogInformation("User {Login} restored by {ModifiedBy}", user.Login, modifiedBy);
        return user;
    }

    private static void ValidateLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login) || !LoginRegex().IsMatch(login))
        {
            throw new ArgumentException("Login can only contain Latin letters and numbers");
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || !PasswordRegex().IsMatch(password))
        {
            throw new ArgumentException("Password can only contain Latin letters and numbers");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !NameRegex().IsMatch(name))
        {
            throw new ArgumentException("Name can only contain Russian and Latin letters");
        }
    }

    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex LoginRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"^[a-zA-Zа-яА-ЯёЁ\s]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex NameRegex();
}