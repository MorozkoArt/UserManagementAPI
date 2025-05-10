using UserManagement.Services;

namespace UserManagement.Middleware;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BasicAuthMiddleware> _logger;

    public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager userManager)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header missing");
            return;
        }

        try
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid authorization header format");
                return;
            }

            var encodedCredentials = authHeader["Basic ".Length..].Trim();
            var decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = decodedCredentials.IndexOf(':');

            if (separatorIndex < 0)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid credentials format");
                return;
            }

            var login = decodedCredentials[..separatorIndex];
            var password = decodedCredentials[(separatorIndex + 1)..];

            var user = await userManager.GetByCredentialsAsync(login, password);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid credentials");
                return;
            }

            context.Items["UserLogin"] = user.Login;
            _logger.LogInformation("User {Login} authenticated", login);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication error");
            return;
        }

        await _next(context);
    }
}
