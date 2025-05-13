using UserManagement.Models;
using UserManagement.Models.Dtos;

namespace UserManagement.Services;

public interface IUserManager
{
    Task<PaginatedResult<User>> GetAllActiveUsersPaginatedAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<User>> GetAllActiveUsersAsync();
    Task<List<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetUsersOlderThanAsync(int age);
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByLoginCachedAsync(string login);
    Task<User?> GetByCredentialsAsync(string login, string password);
    Task<User> CreateUserAsync(UserCreateDto dto, string createdBy);
    Task<User> UpdateUserAsync(string login, UserUpdateDto dto, string modifiedBy);
    Task<User> UpdatePasswordAsync(string login, string newPassword, string modifiedBy);
    Task<User> UpdateLoginAsync(string oldLogin, string newLogin, string modifiedBy);
    Task DeleteUserAsync(string login, string revokedBy, bool softDelete = true);
    Task<User> RestoreUserAsync(string login, string modifiedBy);
}