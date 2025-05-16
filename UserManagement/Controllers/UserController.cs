using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Models.Dtos;
using UserManagement.Services;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class UserController(IUserManager userManager, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<UserController> _logger = logger;

    private async Task<User?> GetCurrentUserAsync()
    {
        var login = User.Identity?.Name;
        return login != null ? await _userManager.GetByLoginAsync(login) : null;
    }

    private BadRequestObjectResult HandleError(Exception ex, string actionName)
    {
        _logger.LogError(ex, "Error in {Action}", actionName);
        return BadRequest(ex.Message);
    }

    private UserAdminResponseDto MapToAdminDto(User user) => new()
    {
        Login = user.Login,
        Name = user.Name,
        Gender = user.Gender,
        Birthday = user.Birthday,
        IsActive = user.IsActive,
        CreatedOn = user.CreatedOn,
        CreatedBy = user.CreatedBy,
        ModifiedOn = user.ModifiedOn,
        ModifiedBy = user.ModifiedBy,
        RevokedOn = user.RevokedOn,
        RevokedBy = user.RevokedBy
    };
}