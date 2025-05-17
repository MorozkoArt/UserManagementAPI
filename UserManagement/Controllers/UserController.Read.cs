using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.Controllers;
public partial class UserController
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllActiveUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _userManager.GetAllActiveUsersPaginatedAsync(page, pageSize);
            var users = result.Items.Select(MapToAdminDto);

            return Ok(new
            {
                Data = users,
                Page = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            });
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetAllActiveUsers));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{login}")]
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        try
        {
            var user = await _userManager.GetByLoginCachedAsync(login);
            return Ok(MapToAdminDto(user));
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (CacheNullReturnException ex)
        {
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetUserByLogin));
        }
    }
    [Authorize]
    [HttpGet("self")]
    public async Task<IActionResult> GetCurrentUserInfo()
    {
        try
        {
            var currentUser = User.Identity?.Name;
            var user = await _userManager.GetCurrentUserAsync(currentUser ?? string.Empty);

            return Ok(new UserResponseDto
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                IsActive = user.IsActive
            });
        }
        catch (Exception ex) when (ex is AuthenticationRequiredException or AccountInactiveException)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetCurrentUserInfo));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("older-than/{age}")]
    public async Task<IActionResult> GetUsersOlderThan(int age, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _userManager.GetUsersOlderPaginatedAsync(age, page, pageSize);
            var users = result.Items.Select(MapToAdminDto);

            return Ok(new
            {
                Data = users,
                Page = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            });
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetUsersOlderThan));
        }
    }
}