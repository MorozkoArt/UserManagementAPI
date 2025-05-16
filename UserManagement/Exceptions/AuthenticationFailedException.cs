namespace UserManagement.Exceptions;

public class AuthenticationFailedException(string message) : Exception(message)
{
}