using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CustomCodeFramework.Api.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomCodeSwagger(
        this IServiceCollection services,
        string title = "API",
        string version = "v1"
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter a valid JWT bearer token.",
                }
            );

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", null, null), [] },
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCustomCodeSwagger(
        this IApplicationBuilder app,
        string routePrefix = "swagger"
    )
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = routePrefix;
        });

        return app;
    }
}
