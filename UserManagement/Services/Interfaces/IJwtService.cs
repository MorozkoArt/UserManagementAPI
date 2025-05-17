using UserManagement.Models;

namespace UserManagement.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}