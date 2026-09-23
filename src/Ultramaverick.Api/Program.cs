using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Identity.Application;
using Ultramaverick.Identity.Infrastructure;
using Ultramaverick.Identity.Infrastructure.Options;
using Ultramaverick.Identity.Infrastructure.Security;
using Ultramaverick.Identity.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityApplication();
builder.Services.AddIdentityPersistence(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Exposed so the authorization test project can host the application.
public partial class Program { }
