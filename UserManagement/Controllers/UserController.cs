using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Models.Dtos;
using UserManagement.Services;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager userManager, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private User? GetCurrentUser()
    {
        var login = HttpContext.Items["UserLogin"] as string;
        return login != null ? _userManager.GetByLogin(login) : null;
    }

    private bool IsAdmin(User? user) => user?.Admin ?? false;

    #region Create
    [HttpPost]
    public IActionResult CreateUser(UserCreateDto dto)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can create users");
            }

            var user = _userManager.CreateUser(dto, currentUser.Login);
            return Ok(new { user.Id, user.Login });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Update-1
    [HttpPut("{login}")]
    public IActionResult UpdateUser(string login, UserUpdateDto dto)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }

            var userToUpdate = _userManager.GetByLogin(login);
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
            {
                return Unauthorized("You can only update your own active account");
            }

            var updatedUser = _userManager.UpdateUser(login, dto, currentUser.Login);
            return Ok(new { updatedUser.Name, updatedUser.Gender, updatedUser.Birthday });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Login}", login);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{login}/password")]
    public IActionResult UpdatePassword(string login, UserPasswordUpdateDto dto)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }

            var userToUpdate = _userManager.GetByLogin(login);
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
            {
                return Unauthorized("You can only update your own active account");
            }

            var updatedUser = _userManager.UpdatePassword(login, dto.NewPassword, currentUser.Login);
            return Ok(new { Message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {Login}", login);
            return BadRequest(ex.Message);
        }
    }
    #endregion
}