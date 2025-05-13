using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.Dtos;

public class UserPasswordUpdateDto
{
    [Required(ErrorMessage = "Password required")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "The password must be between 8 and 64 characters")]
    public required string NewPassword { get; set; }
}