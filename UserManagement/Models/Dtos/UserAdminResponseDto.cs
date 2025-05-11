namespace UserManagement.Models.Dtos;

public class UserAdminResponseDto : UserResponseDto
{
    public required string Login { get; set; }
    public DateTime CreatedOn { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public required string ModifiedBy { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}