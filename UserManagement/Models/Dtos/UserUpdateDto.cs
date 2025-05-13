using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.Dtos;

public class UserUpdateDto
{
    [Required(ErrorMessage = "Name required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "The name should be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "The name can only contain letters, spaces, and hyphens")]
    public required string Name { get; set; }

    public int? Gender { get; set; }
    public DateTime? Birthday { get; set; }
}