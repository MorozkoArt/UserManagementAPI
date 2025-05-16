using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.Controllers;
public partial class UserController
{
    [Authorize]
    [HttpPut("{login}")]
    public async Task<IActionResult> UpdateUser(string login, UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var updatedUser = await _userManager.UpdateUserAsync(login, dto, currentUser?.Login ?? string.Empty);
            return Ok(new { updatedUser.Name, updatedUser.Gender, updatedUser.Birthday });
        }
        catch (AuthenticationRequiredException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (AccountUpdateForbiddenException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdateUser));
        }
    }

    [Authorize]
    [HttpPut("{login}/password")]
    public async Task<IActionResult> UpdatePassword(string login, UserPasswordUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = await GetCurrentUserAsync();
            await _userManager.UpdatePasswordAsync(login, dto.NewPassword, currentUser?.Login ?? string.Empty);
            return Ok(new { Message = "Password updated successfully" });
        }
        catch (AuthenticationRequiredException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (AccountUpdateForbiddenException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdatePassword));
        }
    }

    [Authorize]
    [HttpPut("{login}/login")]
    public async Task<IActionResult> UpdateLogin(string login, UserLoginUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var updatedUser = await _userManager.UpdateLoginAsync(login, dto.NewLogin, currentUser?.Login ?? string.Empty);
            return Ok(new { OldLogin = login, NewLogin = updatedUser.Login });
        }
        catch (AuthenticationRequiredException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (AccountUpdateForbiddenException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (LoginAlreadyExistsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(UpdateLogin));
        }
    }
}