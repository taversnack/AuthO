using Hellang.Middleware.ProblemDetails;
using STSL.SmartLocker.Utils.Api;
using STSL.SmartLocker.Utils.Api.Auth;
using STSL.SmartLocker.Utils.Data.Services;
using STSL.SmartLocker.Utils.Data.Services.SqlServer;
using STSL.SmartLocker.Utils.Data.SqlServer;

var builder = WebApplication.CreateBuilder(args);

#region Add and configure services
// Add services to the collection

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddAndConfigureControllers();

// ServiceExtensions methods
// Convert exceptions and API responses to RFC7807 format
builder.Services.AddProblemDetailsApiResponses();

// Add and setup authentication and authorization policies
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

// Add and configure SQL Server Database
builder.Services.AddSqlServerDatabase(builder.Configuration);

// Add SQL Server specific service implementations
builder.Services.AddSqlServerApplicationServices();

// Add services for application logic 
builder.Services.AddApplicationServices();

// Add options for services
builder.Services.AddServiceOptions(builder.Configuration);

// Adds and configures API versioning and explorer
builder.Services.AddApiVersioningWithSwaggerExplorer();

// Adds and configures CORS for Web Client
builder.Services.AddWebClientCors(builder.Configuration);

#endregion Add and configure services

// Build Web Host
var app = builder.Build();

#region Add and configure middleware
// Configure the HTTP request pipeline. Always ensure middleware runs in correct order:
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0#middleware-order
app.UseProblemDetails();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.UseTenantAuthorizedEndpoints();

app.MapControllers();

#endregion Add and configure middleware

app.Run();
