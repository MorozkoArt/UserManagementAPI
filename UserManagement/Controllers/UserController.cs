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

    private async Task<User?> GetCurrentUserAsync()
    {
        var login = HttpContext.Items["UserLogin"] as string;
        return login != null ? await _userManager.GetByLoginAsync(login) : null;
    }

    private bool IsAdmin(User? user) => user?.Admin ?? false;

    #region Create
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can create users");
            }

            var user = await _userManager.CreateUserAsync(dto, currentUser.Login);
            return Ok(new { user.Id, user.Login });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Update
    [HttpPut("{login}")]
    public async Task<IActionResult> UpdateUser(string login, UserUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }

            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
            {
                return Unauthorized("You can only update your own active account");
            }

            var updatedUser = await _userManager.UpdateUserAsync(login, dto, currentUser.Login);
            return Ok(new { updatedUser.Name, updatedUser.Gender, updatedUser.Birthday });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Login}", login);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{login}/password")]
    public async Task<IActionResult> UpdatePassword(string login, UserPasswordUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }

            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
            {
                return Unauthorized("You can only update your own active account");
            }

            await _userManager.UpdatePasswordAsync(login, dto.NewPassword, currentUser.Login);
            return Ok(new { Message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {Login}", login);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{login}/login")]
    public async Task<IActionResult> UpdateLogin(string login, UserLoginUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }
            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
            {
                return Unauthorized("You can only update your own active account");
            }

            var updatedUser = await _userManager.UpdateLoginAsync(login, dto.NewLogin, currentUser.Login);
            return Ok(new { OldLogin = login, NewLogin = updatedUser.Login });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating login for user {Login}", login);
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Read
    [HttpGet]
    public IActionResult GetAllActiveUsers()
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can get all active users");
            }

            var users = _userManager.GetAllActiveUsers()
                .Select(u => new UserAdminResponseDto
                {
                    Login = u.Login,
                    Name = u.Name,
                    Gender = u.Gender,
                    Birthday = u.Birthday,
                    IsActive = u.IsActive,
                    CreatedOn = u.CreatedOn,
                    CreatedBy = u.CreatedBy,
                    ModifiedOn = u.ModifiedOn,
                    ModifiedBy = u.ModifiedBy,
                    RevokedOn = u.RevokedOn,
                    RevokedBy = u.RevokedBy
                });

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all active users");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{login}")]
    public IActionResult GetUserByLogin(string login)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can get user by login");
            }

            var user = _userManager.GetByLogin(login);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var response = new UserAdminResponseDto
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

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by login {Login}", login);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("self")]
    public IActionResult GetCurrentUserInfo()
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized("Authentication required");
            }

            if (!currentUser.IsActive)
            {
                return Unauthorized("Your account is inactive");
            }

            var response = new UserResponseDto
            {
                Name = currentUser.Name,
                Gender = currentUser.Gender,
                Birthday = currentUser.Birthday,
                IsActive = currentUser.IsActive
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user info");
            return BadRequest(ex.Message);
        }
    }
    

    [HttpGet("older-than/{age}")]
    public IActionResult GetUsersOlderThan(int age)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can get users older than specified age");
            }

            var users = _userManager.GetUsersOlderThan(age)
                .Select(u => new UserAdminResponseDto
                {
                    Login = u.Login,
                    Name = u.Name,
                    Gender = u.Gender,
                    Birthday = u.Birthday,
                    IsActive = u.IsActive,
                    CreatedOn = u.CreatedOn,
                    CreatedBy = u.CreatedBy,
                    ModifiedOn = u.ModifiedOn,
                    ModifiedBy = u.ModifiedBy,
                    RevokedOn = u.RevokedOn,
                    RevokedBy = u.RevokedBy
                });

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users older than {Age}", age);
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Delete
    [HttpDelete("{login}")]
    public IActionResult DeleteUser(string login, [FromQuery] bool softDelete = true)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can delete users");
            }

            _userManager.DeleteUser(login, currentUser.Login, softDelete);
            return Ok(new { Message = softDelete ? "User soft deleted" : "User permanently deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Login}", login);
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Update-2
    [HttpPatch("{login}/restore")]
    public IActionResult RestoreUser(string login)
    {
        try
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || !currentUser.Admin)
            {
                return Unauthorized("Only admin can restore users");
            }

            var restoredUser = _userManager.RestoreUser(login, currentUser.Login);
            return Ok(new { restoredUser.Name, restoredUser.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring user {Login}", login);
            return BadRequest(ex.Message);
        }
    }
    #endregion
}

