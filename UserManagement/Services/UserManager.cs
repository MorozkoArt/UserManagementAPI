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
    private readonly IJwtService  _jwtService;
    private const string AllActiveUsersCacheKey = "all_active_users";
    private const string AllUsersCacheKey = "all_users";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public UserManager(ILogger<UserManager> logger, IMemoryCache cache, IJwtService jwtService)
    {
        _logger = logger;
        _cache = cache;
        _jwtService = jwtService;
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
            CreatedBy = "_System_",
            ModifiedBy = "_System_"
        };

        _users.Add(admin.Login, admin);
        _logger.LogInformation("Admin user initialized");
    }

    public async Task<string> AuthenticateAsync(string login, string password)
    {
        var user = await GetByCredentialsAsync(login, password) ?? throw new AuthenticationFailedException();
        return _jwtService.GenerateToken(user);
    }
    
    #region Create
    public async Task<User> CreateUserAsync(UserCreateDto dto, string createdBy)
    {
        return await Task.Run(async() =>
        {
            var (loginValid, loginError) = UserValidation.ValidateLogin(dto.Login, _users);
            if (!loginValid) throw new ValidationException(loginError);
            var (passValid, passError) = UserValidation.ValidatePassword(dto.Password);
            if (!passValid) throw new ValidationException(passError);
            var (nameValid, nameError) = UserValidation.ValidateName(dto.Name);
            if (!nameValid) throw new ValidationException(nameError);
            var (birthdayValid, birthdayError) = UserValidation.ValidateBirthday(dto.Birthday);
            if (!birthdayValid) throw new ValidationException(birthdayError);

            if (dto.Admin)
            {
                var currentUser = await GetByLoginAsync(createdBy);
                if (currentUser == null || !currentUser.Admin)
                {
                    throw new AdminAccessException();
                }
            }

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
    #endregion

    #region Read
    public async Task<PaginatedResult<User>> GetAllActiveUsersPaginatedAsync(int pageNumber = 1, int pageSize = 10)
    {
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

    public async Task<List<User>> GetUsersOlderThanAsync(int age)
    {
        var cutoffDate = DateTime.Today.AddYears(-age);
        return await Task.FromResult(
            _users.Values
                .Where(u => u.Birthday.HasValue && u.Birthday.Value.Date <= cutoffDate)
                .OrderBy(u => u.CreatedOn)
                .ToList())?? [];
    }

    public async Task<PaginatedResult<User>> GetUsersOlderPaginatedAsync(int age, int pageNumber = 1, int pageSize = 10)
    {
        var allUsers = await GetUsersOlderThanAsync(age);
        var totalCount = allUsers.Count;

        pageSize = Math.Min(pageSize, 100);

        var items = allUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<User>(items, pageNumber, pageSize, totalCount);
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

    public async Task<User> GetByLoginCachedAsync(string login)
    {
        var cacheKey = $"user_{login}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);
        })?? throw new CacheNullReturnException();
    }

    public async Task<User?> GetByCredentialsAsync(string login, string password)
    {
        return await Task.FromResult(
            _users.TryGetValue(login, out var user) && 
            PasswordHasher.VerifyPassword(password, user.HashPassword) && 
            user.IsActive ? user : null);
    }
    #endregion

    #region Update-1
    public async Task<User> UpdateUserAsync(string login, UserUpdateDto dto, string modifiedBy)
    {
        var currentUser = await GetByLoginAsync(modifiedBy) ?? throw new AuthenticationRequiredException();
        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        if (!currentUser.Admin && (currentUser.Login != user.Login || !user.IsActive))
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
        var currentUser = await GetByLoginAsync(modifiedBy) ?? throw new AuthenticationRequiredException();
        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        if (!currentUser.Admin && (currentUser.Login != user.Login || !user.IsActive))
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
        var currentUser = await GetByLoginAsync(modifiedBy) ?? throw new AuthenticationRequiredException();
        var user = await GetByLoginAsync(oldLogin) ?? throw new UserNotFoundException(oldLogin);

        if (!currentUser.Admin && (currentUser.Login != user.Login || !user.IsActive))
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
    #endregion

    #region Delete
    public async Task DeleteUserAsync(string login, string revokedBy, bool softDelete = true)
    {
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
    #endregion

    #region Update-2 (Restore)
    public async Task<User> RestoreUserAsync(string login, string modifiedBy)
    {
        var user = await GetByLoginAsync(login) ?? throw new UserNotFoundException(login);

        user.RevokedOn = null;
        user.RevokedBy = null;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = modifiedBy;
        InvalidateCaches(login);

        _logger.LogInformation("User {Login} restored by {ModifiedBy}", user.Login, modifiedBy);

        return await Task.FromResult(user);
    }
    #endregion

    private void InvalidateCaches(params string[] logins)
    {
        _cache.Remove(AllActiveUsersCacheKey);
        _cache.Remove(AllUsersCacheKey);

        foreach (var login in logins)
            _cache.Remove($"user_{login}");
    }
}