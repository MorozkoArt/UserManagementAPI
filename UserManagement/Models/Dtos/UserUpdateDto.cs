using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace UserManagement.Models.Dtos;

public class UserUpdateDto
{
    [Required(ErrorMessage = "Name required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "The name should be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "The name can only contain letters, spaces, and hyphens")]
    public required string Name { get; set; }

    [RegularExpression(@"^[0-2]+$", ErrorMessage = "The gender value can contain only 3 values (0 - women, 1 - man, 2 - unknown)")]
    public int? Gender { get; set; } = 2;

    [DataType(DataType.Date, ErrorMessage = "Invalid date format")]
    [SwaggerSchema(Description = "Date in format YYYY-MM-DD")]
    [Range(typeof(DateTime), "1900-01-01", "2100-01-01", ErrorMessage = "Birthday must be between 1900 and 2100")]
    public DateTime? Birthday { get; set; }
}