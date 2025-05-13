namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;

public partial class UserController
{
    [HttpPut("{login}")]
    public async Task<IActionResult> UpdateUser(string login, UserUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized("Authentication required");

            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
                return NotFound("User not found");

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
                return Unauthorized("You can only update your own active account");

            var updatedUser = await _userManager.UpdateUserAsync(login, dto, currentUser.Login);
            return Ok(new { updatedUser.Name, updatedUser.Gender, updatedUser.Birthday });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdateUser));
        }
    }

    [HttpPut("{login}/password")]
    public async Task<IActionResult> UpdatePassword(string login, UserPasswordUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized("Authentication required");

            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
                return NotFound("User not found");

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
                return Unauthorized("You can only update your own active account");

            await _userManager.UpdatePasswordAsync(login, dto.NewPassword, currentUser.Login);
            return Ok(new { Message = "Password updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdatePassword));
        }
    }

    [HttpPut("{login}/login")]
    public async Task<IActionResult> UpdateLogin(string login, UserLoginUpdateDto dto)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized("Authentication required");

            var userToUpdate = await _userManager.GetByLoginAsync(login);
            if (userToUpdate == null)
                return NotFound("User not found");

            if (!currentUser.Admin && (currentUser.Login != login || !userToUpdate.IsActive))
                return Unauthorized("You can only update your own active account");

            var updatedUser = await _userManager.UpdateLoginAsync(login, dto.NewLogin, currentUser.Login);
            return Ok(new { OldLogin = login, NewLogin = updatedUser.Login });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdateLogin));
        }
    }
}