using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Models.Dtos;
using UserManagement.Services.Interfaces;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class UserController(IUserManager userManager, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<UserController> _logger = logger;

    private NotFoundObjectResult HandleNotFound(Exception ex)
    {
        _logger.LogError("Error: {Message}", ex.Message);
        return NotFound(ex.Message);
    }

    private BadRequestObjectResult HandleBadRequest(Exception ex)
    {
        _logger.LogError("Error: {Message}", ex.Message);
        return BadRequest(ex.Message);
    }

    private UnauthorizedObjectResult HandleUnauthorized(Exception ex)
    {
        _logger.LogError("Error: {Message}", ex.Message);
        return Unauthorized(ex.Message);
    }

    private ForbidResult HandleForbid(Exception ex)
    {
        _logger.LogError("Error: {Message}", ex.Message);
        return Forbid();
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