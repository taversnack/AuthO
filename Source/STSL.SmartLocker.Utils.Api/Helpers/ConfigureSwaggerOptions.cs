using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace STSL.SmartLocker.Utils.Api.Helpers;

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private const string DefaultVersionTitle = "STSL Smart Locker API";
    private const string DefaultVersionDescription = "All API methods are secured with OAuth bearer tokens.";

    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionprovider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        => _apiVersionDescriptionprovider = apiVersionDescriptionProvider;

    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (var description in _apiVersionDescriptionprovider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }

        // add xml documentation
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

        // add bearer token support
        OpenApiSecurityScheme securitySchema = new()
        {
            Description = "Using the Auth header with the Bearer scheme.",
            Name = "Auth",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        options.AddSecurityDefinition("Bearer", securitySchema);

        options.AddSecurityRequirement(new()
        {
            { securitySchema, new[] { "Bearer" } }
        });
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description) => new()
    {
        Title = DefaultVersionTitle,
        Version = description.ApiVersion.ToString(),
        Description = description.IsDeprecated ? DefaultVersionDescription + " This API version has been deprecated." : DefaultVersionDescription
    };
}

internal sealed class ConfigureSwaggerUIOptions : IConfigureOptions<SwaggerUIOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionprovider;

    public ConfigureSwaggerUIOptions(IApiVersionDescriptionProvider apiVersionDescriptionprovider)
        => _apiVersionDescriptionprovider = apiVersionDescriptionprovider;

    public void Configure(SwaggerUIOptions options)
    {
        foreach (var description in _apiVersionDescriptionprovider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()
            );
        }
    }
}