namespace UserManagement.Exceptions;

public class LoginAlreadyExistsException : ArgumentException
{
    public LoginAlreadyExistsException()
        : base("Login already exists") { }

    public LoginAlreadyExistsException(string login)
        : base($"Login: '{login}' - already exists") {}

    public LoginAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }

}