namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;
using UserManagement.Exceptions;


public partial class UserController
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        try
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userManager.CreateUserAsync(dto, currentUser?.Login ?? string.Empty);
            return Ok(new { user.Id, user.Login });
        }
        catch (AdminAccessException ex)
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
            return HandleError(ex, nameof(CreateUser));
        }
    }
}