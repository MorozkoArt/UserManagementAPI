namespace UserManagement.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException()
        : base("User not found") { }

    public UserNotFoundException(string login)
        : base($"User: '{login}' - not found") { }

    public UserNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}