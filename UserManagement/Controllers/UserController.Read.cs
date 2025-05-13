namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;


public partial class UserController
{
    [HttpGet]
    public async Task<IActionResult> GetAllActiveUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can get all active users");

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

    [HttpGet("{login}")]
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can get user by login");

            var user = await _userManager.GetByLoginCachedAsync(login);
            return user == null 
                ? NotFound("User not found") 
                : Ok(MapToAdminDto(user));
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
            if (currentUser == null)
                return Unauthorized("Authentication required");
            if (!currentUser.IsActive)
                return Unauthorized("Your account is inactive");

            return Ok(new UserResponseDto
            {
                Name = currentUser.Name,
                Gender = currentUser.Gender,
                Birthday = currentUser.Birthday,
                IsActive = currentUser.IsActive
            });
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
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can get users older than specified age");

            var users = await _userManager.GetUsersOlderThanAsync(age);
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
        catch (Exception ex)
        {
            return HandleError(ex, nameof(GetUsersOlderThan));
        }
    }
}