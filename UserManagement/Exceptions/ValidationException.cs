namespace UserManagement.Exceptions;

public class ValidationException : ArgumentException
{
    public ValidationException(string message)
        : base(message) { }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}