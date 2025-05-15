namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;


public partial class UserController
{
    [HttpGet]
    public async Task<IActionResult> GetAllActiveUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var result = await _userManager.GetAllActiveUsersPaginatedAsync(currentUser?.Login ?? string.Empty, page, pageSize);
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
        catch (AdminAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetAllActiveUsers));
        }
    }

    [HttpGet("{login}")]
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userManager.GetByLoginCachedAsync(login, currentUser?.Login ?? string.Empty);
            return user == null 
                ? NotFound("User not found") 
                : Ok(MapToAdminDto(user));
        }
        catch (AdminAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetUserByLogin));
        }
    }

    [HttpGet("self")]
    public async Task<IActionResult> GetCurrentUserInfo()
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userManager.GetCurrentUserAsync(currentUser?.Login ?? string.Empty);

            return Ok(new UserResponseDto
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                IsActive = user.IsActive
            });
        }
        catch (AuthenticationRequiredException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (AccountInactiveException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetCurrentUserInfo));
        }
    }

    [HttpGet("older-than/{age}")]
    public async Task<IActionResult> GetUsersOlderThan(int age, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var users = await _userManager.GetUsersOlderThanAsync(age, currentUser?.Login ?? string.Empty);
            var paginatedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToAdminDto);

            return Ok(new
            {
                Data = paginatedUsers,
                Page = page,
                PageSize = pageSize,
                TotalCount = users.Count(),
                TotalPages = (int)Math.Ceiling(users.Count() / (double)pageSize)
            });
        }
        catch (AdminAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetUsersOlderThan));
        }
    }
}