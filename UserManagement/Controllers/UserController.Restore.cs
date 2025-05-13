namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;

public partial class UserController
{
    [HttpPatch("{login}/restore")]
    public async Task<IActionResult> RestoreUser(string login)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can restore users");

            var restoredUser = await _userManager.RestoreUserAsync(login, currentUser.Login);
            return Ok(new { restoredUser.Name, restoredUser.IsActive });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(RestoreUser));
        }
    }
}