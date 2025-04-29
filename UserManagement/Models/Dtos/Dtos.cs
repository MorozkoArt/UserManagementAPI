namespace UserManagement.Models.Dtos;

public class UserCreateDto
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
}

public class UserUpdateDto
{
    public string? Name { get; set; }
    public int? Gender { get; set; }
    public DateTime? Birthday { get; set; }
}

public class UserPasswordUpdateDto
{
    public required string NewPassword { get; set; }
}

public class UserLoginUpdateDto
{
    public required string NewLogin { get; set; }
}

public class UserResponseDto
{
    public required string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsActive { get; set; }
}

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