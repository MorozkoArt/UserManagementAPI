namespace UserManagement.Controllers;
using Microsoft.AspNetCore.Mvc;

public partial class UserController
{
    [HttpDelete("{login}")]
    public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool softDelete = true)
    {
        try
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.Admin)
                return Unauthorized("Only admin can delete users");

            await _userManager.DeleteUserAsync(login, currentUser.Login, softDelete);
            return Ok(new { Message = softDelete ? "User soft deleted" : "User permanently deleted" });
        }
        catch (Exception ex)
        {
            return HandleError(ex, nameof(DeleteUser));
        }
    }
}