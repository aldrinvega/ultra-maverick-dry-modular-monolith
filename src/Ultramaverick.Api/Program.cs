using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Identity.Application;
using Ultramaverick.Identity.Infrastructure;
using Ultramaverick.Identity.Infrastructure.Options;
using Ultramaverick.Identity.Infrastructure.Security;
using Ultramaverick.Api.Integration;
using Ultramaverick.Catalog.Application;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Infrastructure;
using Ultramaverick.Catalog.Persistence;
using Ultramaverick.Identity.Persistence;
using Ultramaverick.Procurement.Application;
using Ultramaverick.Procurement.Infrastructure;
using Ultramaverick.Procurement.Persistence;
using Ultramaverick.Quality.Application;
using Ultramaverick.Quality.Infrastructure;
using Ultramaverick.Quality.Persistence;
using Ultramaverick.Laboratory.Application;
using Ultramaverick.Laboratory.Infrastructure;
using Ultramaverick.Laboratory.Persistence;
using Ultramaverick.Warehouse.Application;
using Ultramaverick.Warehouse.Infrastructure;
using Ultramaverick.Warehouse.Persistence;
using Ultramaverick.Orders.Application;
using Ultramaverick.Orders.Infrastructure;
using Ultramaverick.Orders.Persistence;
using Ultramaverick.Manufacturing.Application;
using Ultramaverick.Manufacturing.Infrastructure;
using Ultramaverick.Manufacturing.Persistence;
using Ultramaverick.Reporting.Application;
using Ultramaverick.Reporting.Infrastructure;
using Ultramaverick.Reporting.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityApplication();
builder.Services.AddIdentityPersistence(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddCatalogApplication();
builder.Services.AddCatalogPersistence(builder.Configuration);
builder.Services.AddCatalogInfrastructure(builder.Configuration);

builder.Services.AddProcurementApplication();
builder.Services.AddProcurementPersistence(builder.Configuration);
builder.Services.AddProcurementInfrastructure(builder.Configuration);

builder.Services.AddQualityApplication();
builder.Services.AddQualityPersistence(builder.Configuration);
builder.Services.AddQualityInfrastructure(builder.Configuration);

builder.Services.AddLaboratoryApplication();
builder.Services.AddLaboratoryPersistence(builder.Configuration);
builder.Services.AddLaboratoryInfrastructure(builder.Configuration);

builder.Services.AddWarehouseApplication();
builder.Services.AddWarehousePersistence(builder.Configuration);
builder.Services.AddWarehouseInfrastructure(builder.Configuration);

builder.Services.AddOrdersApplication();
builder.Services.AddOrdersPersistence(builder.Configuration);
builder.Services.AddOrdersInfrastructure(builder.Configuration);

builder.Services.AddManufacturingApplication();
builder.Services.AddManufacturingPersistence(builder.Configuration);
builder.Services.AddManufacturingInfrastructure(builder.Configuration);

builder.Services.AddReportingApplication();
builder.Services.AddReportingPersistence(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);

// Bridges Catalog's cross-context department port to Identity's query surface.
builder.Services.AddScoped<IDepartmentDirectory, CatalogDepartmentDirectory>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Ultramaverick.Api.Telemetry.TelemetryBehavior<,>));

// Model-validation failures use the shared validation error shape.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .SelectMany(e => e.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(new ApiValidationError(errors));
    };
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var modules = new[]
{
    "Identity", "Catalog", "Procurement", "Quality", "Laboratory",
    "Warehouse", "Orders", "Manufacturing", "Reporting"
};

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.IdentityAdmin, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(JwtTokenService.ModuleClaimType, "Identity"));

    foreach (var module in modules)
    {
        var name = module;
        options.AddPolicy(Policies.RequireModule(name), policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(JwtTokenService.ModuleClaimType, name));
    }
});

// Throttle authentication attempts per client address.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();

// A migrated or administrator-reset account signs in only to replace its password.
// The claim is issued by JwtTokenService and cleared when the password changes.
app.Use(async (context, next) =>
{
    var mustChange = context.User.Identity?.IsAuthenticated == true &&
                     context.User.FindFirst(JwtTokenService.MustChangePasswordClaimType)?.Value == "true";

    if (mustChange &&
        !context.Request.Path.Equals("/api/Login/change-password", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.Path.Equals("/api/Login/refresh", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.Path.Equals("/api/Login/revoke", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new ApiResponse(403, "Password change required."));
        return;
    }

    await next();
});

app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

// Exposed so the authorization test project can host the application.
public partial class Program { }
