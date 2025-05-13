namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models.Dtos;


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
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can create users");

            var user = await _userManager.CreateUserAsync(dto, currentUser.Login);
            return Ok(new { user.Id, user.Login });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(CreateUser));
        }
    }
}