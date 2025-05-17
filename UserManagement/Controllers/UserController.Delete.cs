using Microsoft.AspNetCore.Mvc;
using UserManagement.Exceptions;
using Microsoft.AspNetCore.Authorization;


namespace UserManagement.Controllers;
public partial class UserController
{
    [Authorize(Roles = "Admin")]
    [HttpDelete("{login}")]
    public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool softDelete = true)
    {
        try
        {
            var currentUser = User.Identity?.Name;
            await _userManager.DeleteUserAsync(login, currentUser ?? string.Empty, softDelete);
            return Ok(new { Message = softDelete ? "User soft deleted" : "User permanently deleted" });
        }
        catch (UserNotFoundException ex)
        {
            return HandleNotFound(ex);
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(DeleteUser));
        }
    }
}