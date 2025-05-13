using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserManagement.Models.Dtos;

namespace UserManagement.Utilities;
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(UserCreateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["login"] = new OpenApiString("user123"),
                ["password"] = new OpenApiString("SecurePass123!"),
                ["name"] = new OpenApiString("Ivan Ivanov"),
                ["gender"] = new OpenApiInteger(0),
                ["birthday"] = new OpenApiString("1990-01-01"),
                ["admin"] = new OpenApiBoolean(false)
            };
        }

        else if (context.Type == typeof(UserUpdateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("Ivan Ivanov"),
                ["gender"] = new OpenApiInteger(0),
                ["birthday"] = new OpenApiString("1990-01-01"),
            };
        }

        else if (context.Type == typeof(UserLoginUpdateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["newlogin"] = new OpenApiString("user123"),
            };
        }

        else if (context.Type == typeof(UserPasswordUpdateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["newPassword"] = new OpenApiString("SecurePass123!"),
            };
        }
    }
}