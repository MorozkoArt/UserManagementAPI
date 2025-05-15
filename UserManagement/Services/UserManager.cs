using Microsoft.Extensions.Caching.Memory;
using UserManagement.Models;
using UserManagement.Models.Dtos;
using UserManagement.Utilities;
using UserManagement.Exceptions;

namespace UserManagement.Services;

public sealed class UserManager : IUserManager
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
            HashPassword = PasswordHasher.HashPassword("Admin_123"),
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

    public async Task<bool> IsAdminUserAsync(string login)
    {
        var user = await GetByLoginAsync(login);
        return user != null && user.Admin;
    }
    public async Task<bool> IsFoundUserAsync(string login)
    {
        var user = await GetByLoginAsync(login);
        return user != null;
    }
    public async Task<bool> IsYourAccountAsync(string currentlogin, string login)
    {
        var currentUser = await GetByLoginAsync(currentlogin);
        var userToUpdate = await GetByLoginAsync(login);
        
        if (currentUser == null || userToUpdate == null)
            return false;

        return currentUser.Admin || (currentUser.Login == login && userToUpdate.IsActive);
    }

    public async Task<PaginatedResult<User>> GetAllActiveUsersPaginatedAsync(string currentUser, int pageNumber = 1, int pageSize = 10)
    {
        if (!await IsAdminUserAsync(currentUser))
            throw new AdminAccessException("Only admin can get all users");

        var allUsers = await GetAllActiveUsersAsync();
        var totalCount = allUsers.Count;
        
        pageSize = Math.Min(pageSize, 100);
        
        var items = allUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<User>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<List<User>> GetAllActiveUsersAsync()
    {
        return await _cache.GetOrCreateAsync(AllActiveUsersCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return Task.FromResult(_users.Values
                .Where(u => u.IsActive)
                .OrderBy(u => u.CreatedOn)
                .ToList());
        }) ?? [];
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _cache.GetOrCreateAsync(AllUsersCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return Task.FromResult(_users.Values
                .OrderBy(u => u.CreatedOn)
                .ToList());
        }) ?? [];
    }

    public async Task<IEnumerable<User>> GetUsersOlderThanAsync(int age, string currentUser)
    {
        if (!await IsAdminUserAsync(currentUser))
            throw new AdminAccessException("Only admin can get users older than specified age");
        var cutoffDate = DateTime.Today.AddYears(-age);
        return await Task.FromResult(
            _users.Values
                .Where(u => u.Birthday.HasValue && u.Birthday.Value.Date <= cutoffDate)
                .OrderBy(u => u.CreatedOn));
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await Task.FromResult(_users.TryGetValue(login, out var user) ? user : null);
    }

    public async Task<User> GetCurrentUserAsync(string login)
    {
        var user = await GetByLoginAsync(login) ?? throw new AuthenticationRequiredException();
        if (!user.IsActive)
            throw new AccountInactiveException();
        return user;
    }



    public async Task<User?> GetByLoginCachedAsync(string login, string currentUser)
    {
        var cacheKey = $"user_{login}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            if (!await IsAdminUserAsync(currentUser))
                throw new AdminAccessException("Only admin can get user by login");
            
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return await GetByLoginAsync(login);
        });
    }

    public async Task<User?> GetByCredentialsAsync(string login, string password)
    {
        return await Task.FromResult(
            _users.TryGetValue(login, out var user) && 
            PasswordHasher.VerifyPassword(password, user.HashPassword) && 
            user.IsActive ? user : null);
    }

    public async Task<User> CreateUserAsync(UserCreateDto dto, string createdBy)
    {
        return await Task.Run(async () =>
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(createdBy);

            if (!await IsAdminUserAsync(createdBy))
                throw new AdminAccessException("Only admin can create users");

            var (loginValid, loginError) = UserValidation.ValidateLogin(dto.Login, _users);
            if (!loginValid) throw new ValidationException(loginError);

            var (passValid, passError) = UserValidation.ValidatePassword(dto.Password);
            if (!passValid) throw new ValidationException(passError);
            var (nameValid, nameError) = UserValidation.ValidateName(dto.Name);
            if (!nameValid) throw new ValidationException(nameError);
            var (birthdayValid, birthdayError) = UserValidation.ValidateBirthday(dto.Birthday);
            if (!birthdayValid) throw new ValidationException(birthdayError);

            if (_users.ContainsKey(dto.Login))
                throw new LoginAlreadyExistsException(dto.Login);

            var user = new User
            {
                Login = dto.Login,
                HashPassword = PasswordHasher.HashPassword(dto.Password),
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
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        if (!await IsFoundUserAsync(modifiedBy))
            throw new AuthenticationRequiredException();

        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        if (!await IsYourAccountAsync(modifiedBy, login))
            throw new AccountUpdateForbiddenException();

        var (nameValid, nameError) = UserValidation.ValidateName(dto.Name);
        if (!nameValid) throw new ValidationException(nameError);
        user.Name = dto.Name;

        if (dto.Gender.HasValue)
            user.Gender = dto.Gender.Value;

        if (dto.Birthday.HasValue)
        {
            var (birthdayValid, birthdayError) = UserValidation.ValidateBirthday(dto.Birthday);
            if (!birthdayValid) throw new ValidationException(birthdayError);
            user.Birthday = dto.Birthday;
        }

        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        InvalidateCaches(login);

        _logger.LogInformation("User {Login} updated by {ModifiedBy}", user.Login, modifiedBy);

        return await Task.FromResult(user);
    }

    public async Task<User> UpdatePasswordAsync(string login, string newPassword, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(newPassword);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        if (!await IsFoundUserAsync(modifiedBy))
            throw new AuthenticationRequiredException();

        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        if (!await IsYourAccountAsync(modifiedBy, login))
            throw new AccountUpdateForbiddenException();

        var (passValid, passError) = UserValidation.ValidatePassword(newPassword);
        if (!passValid) throw new ValidationException(passError);

        user.HashPassword = PasswordHasher.HashPassword(newPassword);;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        InvalidateCaches(login);

        _logger.LogInformation("Password for user {Login} updated by {ModifiedBy}", user.Login, modifiedBy);

        return await Task.FromResult(user);
    }

    public async Task<User> UpdateLoginAsync(string oldLogin, string newLogin, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(newLogin);
        ArgumentNullException.ThrowIfNull(modifiedBy);

        if (!await IsFoundUserAsync(modifiedBy))
            throw new AuthenticationRequiredException();

        var user = await GetByLoginAsync(oldLogin) ?? throw new UserNotFoundException(oldLogin);

        if (!await IsYourAccountAsync(modifiedBy, oldLogin))
            throw new AccountUpdateForbiddenException();
        
        var (loginValid, loginError) = UserValidation.ValidateLogin(newLogin, _users);
        if (!loginValid) throw new ValidationException(loginError);

        if (_users.ContainsKey(newLogin))
            throw new LoginAlreadyExistsException(newLogin);

        _users.Remove(oldLogin);
        user.Login = newLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        _users.Add(newLogin, user);
        InvalidateCaches(newLogin, oldLogin);

        _logger.LogInformation("Login for user {OldLogin} changed to {NewLogin} by {ModifiedBy}", 
            oldLogin, newLogin, modifiedBy);

        return await Task.FromResult(user);
    }

    public async Task DeleteUserAsync(string login, string revokedBy, bool softDelete = true)
    {
        ArgumentNullException.ThrowIfNull(revokedBy);

        if (!await IsAdminUserAsync(revokedBy))
            throw new AdminAccessException("Only admin can delete users");

        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

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


    }

    public async Task<User> RestoreUserAsync(string login, string modifiedBy)
    {
        ArgumentNullException.ThrowIfNull(modifiedBy);

        if (!await IsAdminUserAsync(modifiedBy))
            throw new AdminAccessException("Only admin can restore users");

        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        user.RevokedOn = null;
        user.RevokedBy = null;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        InvalidateCaches(login);

        _logger.LogInformation("User {Login} restored by {ModifiedBy}", user.Login, modifiedBy);

        return await Task.FromResult(user);

    }

    private void InvalidateCaches(params string[] logins)
    {
        _cache.Remove(AllActiveUsersCacheKey);
        _cache.Remove(AllUsersCacheKey);
        
        foreach (var login in logins)
            _cache.Remove($"user_{login}");
    }
}