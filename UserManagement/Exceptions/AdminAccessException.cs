namespace UserManagement.Exceptions;

public class AdminAccessException : UnauthorizedAccessException
{
    public AdminAccessException()
        : base("Only admins can create admin users") { }

    public AdminAccessException(string message)
        : base(message) { }

    public AdminAccessException(string message, Exception innerException)
        : base(message, innerException) { }
}