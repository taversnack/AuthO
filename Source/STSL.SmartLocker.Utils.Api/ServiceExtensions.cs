using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Api.Helpers;
using STSL.SmartLocker.Utils.Common.Exceptions;
using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Api;

internal static class ServiceExtensions
{
    public static void AddAndConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers(options => options.Conventions.Add(
            new RouteTokenTransformerConvention(new SlugifyParameterTransformer()))
        ).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.AllowTrailingCommas = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    }

    public static void AddProblemDetailsApiResponses(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.Map<BaseException>(exception =>
            {
                ProblemDetails problemDetails = new()
                {
                    Type = exception.Type,
                    Title = exception.Title,
                    Status = exception.Status,
                    Detail = exception.Detail,
                    Instance = exception.Instance
                };

                if (exception.Extensions is not null)
                {
                    foreach (var (key, value) in exception.Extensions)
                    {
                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            problemDetails.Extensions.TryAdd(key, value);
                        }
                    }
                }

                return problemDetails;
            });

            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

            options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

            // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
            // If an exception other than ProblemDetailsException, NotImplementedException or HttpRequestException is thrown, this will handle it.
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        });
    }

    public static void AddApiVersioningWithSwaggerExplorer(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SupportNonNullableReferenceTypes();
        });

        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.ConfigureOptions<ConfigureSwaggerUIOptions>();

        services.AddApiVersioningWithExplorer();
    }

    private static void AddApiVersioningWithExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    public static void AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication().AddJwtBearer(options =>
        {
            options.TokenValidationParameters.RoleClaimType = Claims.Roles;
        });

        var issuer = configuration.GetRequiredSection("Authentication:Schemes:Bearer")["ValidIssuer"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new Exception("No ValidIssuer supplied in configuration");
        }

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.Policies)
            {
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireClaim(Claims.Permissions, permission);
                    policy.Requirements.Add(new ScopeRequirement(issuer, permission));
                });
            }
        });

        services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
    }

    public static void AddWebClientCors(this IServiceCollection services, IConfiguration configuration)
        => services.AddCors(options =>
            options.AddDefaultPolicy(policy => policy
                .DisallowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins(configuration.GetRequiredSection("WebClient:Urls").Get<string[]>() ?? throw new("WebClient:Urls missing!"))));
}
