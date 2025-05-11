using Microsoft.Extensions.Caching.Memory;
using UserManagement.Models;
using UserManagement.Models.Dtos;
using System.Text.RegularExpressions;

namespace UserManagement.Services;

public sealed partial class UserManager
{
    private readonly Dictionary<string, User> _users = [];
    private readonly ILogger<UserManager> _logger;
    private readonly IMemoryCache _cache;
    private const string AllActiveUsersCacheKey = "all_active_users";
    private const string AllUsersCacheKey = "all_users";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public UserManager(ILogger<UserManager> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
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

    public async Task<PaginatedResult<User>> GetAllActiveUsersPaginatedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var allUsers = await GetAllActiveUsersAsync();
        var totalCount = allUsers.Count();
        
        pageSize = Math.Min(pageSize, 100); // Максимум 100 на страницу, возможно потом нужно будет изменить
        
        var items = allUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<User>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<List<User>> GetAllActiveUsersAsync()
    {
        var result = _cache.GetOrCreate(AllActiveUsersCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return _users.Values
                .Where(u => u.IsActive)
                .OrderBy(u => u.CreatedOn)
                .ToList();
        });
        
        return await Task.FromResult(result ?? new List<User>());
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var result = _cache.GetOrCreate(AllUsersCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return _users.Values
                .OrderBy(u => u.CreatedOn)
                .ToList();
        });
        
        return await Task.FromResult(result ?? new List<User>());
    }

    public async Task<IEnumerable<User>> GetUsersOlderThanAsync(int age)
    {
        var cutoffDate = DateTime.Today.AddYears(-age);
        return await Task.FromResult(
            _users.Values
                .Where(u => u.Birthday.HasValue && u.Birthday.Value.Date <= cutoffDate)
                .OrderBy(u => u.CreatedOn)
                .ToList());
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await Task.FromResult(_users.TryGetValue(login, out var user) ? user : null);
    }

    public async Task<User?> GetByLoginCachedAsync(string login)
    {
        var cacheKey = $"user_{login}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return await GetByLoginAsync(login);
        });
    }

    public async Task<User?> GetByCredentialsAsync(string login, string password)
    {
        return await Task.FromResult(
            _users.TryGetValue(login, out var user) && 
            user.Password == password && 
            user.IsActive ? user : null);
    }

    public async Task<bool> LoginExistsAsync(string login)
    {
        return await Task.FromResult(_users.ContainsKey(login));
    }


    public async Task<User> CreateUserAsync(UserCreateDto dto, string createdBy)
    {
        return await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(createdBy);

            ValidateLogin(dto.Login);
            ValidatePassword(dto.Password);
            ValidateName(dto.Name);

            if (_users.ContainsKey(dto.Login))
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
            InvalidateCaches(user.Login);
            
            _logger.LogInformation("User {Login} created by {CreatedBy}", user.Login, createdBy);
            return user;
        });
    }

    public async Task<User> UpdateUserAsync(string login, UserUpdateDto dto, string modifiedBy)
    {
        return await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(modifiedBy);

            var user = _users.TryGetValue(login, out var u) ? u : throw new KeyNotFoundException("User not found");

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
            InvalidateCaches(login);

            _logger.LogInformation("User {Login} updated by {ModifiedBy}", user.Login, modifiedBy);
            return user;
        });
    }

    public async Task<User> UpdatePasswordAsync(string login, string newPassword, string modifiedBy)
    {
        return await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(newPassword);
            ArgumentNullException.ThrowIfNull(modifiedBy);

            var user = _users.TryGetValue(login, out var u) ? u : throw new KeyNotFoundException("User not found");
            ValidatePassword(newPassword);

            user.Password = newPassword;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            InvalidateCaches(login);

            _logger.LogInformation("Password for user {Login} updated by {ModifiedBy}", user.Login, modifiedBy);
            return user;
        });
    }

    public async Task<User> UpdateLoginAsync(string oldLogin, string newLogin, string modifiedBy)
    {
        return await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(newLogin);
            ArgumentNullException.ThrowIfNull(modifiedBy);

            var user = _users.TryGetValue(oldLogin, out var u) ? u : throw new KeyNotFoundException("User not found");
            ValidateLogin(newLogin);

            if (_users.ContainsKey(newLogin))
            {
                throw new ArgumentException("New login already exists");
            }

            _users.Remove(oldLogin);
            user.Login = newLogin;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            _users.Add(newLogin, user);
            InvalidateCaches(newLogin, oldLogin);

            _logger.LogInformation("Login for user {OldLogin} changed to {NewLogin} by {ModifiedBy}", 
                oldLogin, newLogin, modifiedBy);
            return user;
        });
    }


    public async Task DeleteUserAsync(string login, string revokedBy, bool softDelete = true)
    {
        await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(revokedBy);

            var user = _users.TryGetValue(login, out var u) ? u : throw new KeyNotFoundException("User not found");

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

            InvalidateCaches(login);
        });
    }

    public async Task<User> RestoreUserAsync(string login, string modifiedBy)
    {
        return await Task.Run(() =>
        {
            ArgumentNullException.ThrowIfNull(modifiedBy);

            var user = _users.TryGetValue(login, out var u) ? u : throw new KeyNotFoundException("User not found");

            user.RevokedOn = null;
            user.RevokedBy = null;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;
            InvalidateCaches(login);

            _logger.LogInformation("User {Login} restored by {ModifiedBy}", user.Login, modifiedBy);
            return user;
        });
    }

    private void InvalidateCaches(params string[] logins)
    {
        _cache.Remove(AllActiveUsersCacheKey);
        _cache.Remove(AllUsersCacheKey);
        
        foreach (var login in logins)
        {
            _cache.Remove($"user_{login}");
        }
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
