using FoodHub.Api.GraphQL.Mutations;
using FoodHub.Api.GraphQL.Queries;
using FoodHub.Api.Auth.Google;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using FoodHub.Restaurant.Infrastructure.Persistence.Repositories;
// ...existing code...
using Azure.Identity;
using System.Text;
using Serilog.Events;
using Microsoft.Azure.Cosmos;
// ...existing code...
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
// Azure Key Vault configuration removed (Azure.Identity not available)
// Azure Key Vault Configuration (Production only)
// if (builder.Environment.IsDevelopment())
{
    var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];
    if (!string.IsNullOrEmpty(keyVaultEndpoint))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultEndpoint),
            new DefaultAzureCredential());
    }
}

// Bind options for both modules from the single top-level "Cosmos" section
var cosmosSection = builder.Configuration.GetSection("Cosmos");
builder.Services.Configure<FoodHub.Restaurant.Infrastructure.Persistence.Cosmos.CosmosOptions>(cosmosSection);

// Build shared CosmosClient from top-level Cosmos credentials
var endpoint = cosmosSection.GetValue<string>("Endpoint") ?? throw new InvalidOperationException("Cosmos:Endpoint is not configured.");
var key = cosmosSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Cosmos:Key is not configured.");

// Cosmos client (shared instance)
builder.Services.AddSingleton(new CosmosClient(endpoint, key));

// Contexts (module-specific)
builder.Services.AddSingleton<FoodHub.Restaurant.Infrastructure.Persistence.Cosmos.CosmosContext>();

// Repositories: register Restaurant repository only
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

// Auth services
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("GoogleAuth"));
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<FoodHub.Api.Auth.JWT.IJwtTokenGenerator, FoodHub.Api.Auth.JWT.JwtTokenGenerator>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT Secret is not configured");
var issuer = jwtSection.GetValue<string>("Issuer") ?? "FoodHub";
var audience = jwtSection.GetValue<string>("Audience") ?? "FoodHub";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

builder.Services.AddAuthorization();

// Add Controllers for auth endpoints
builder.Services.AddControllers();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType()
    .AddTypeExtension<RestaurantQuery>()
    .AddMutationType()
    .AddTypeExtension<RestaurantMutation>();

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();

// Expose Serilog's logger for code that requires Serilog-specific APIs
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

builder.Host.UseSerilog();

var app = builder.Build();

// ...existing code...

// Correlation ID Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers.Append("X-Correlation-ID", correlationId);

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

// Inline middleware to log user identity and claims
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user?.Identity != null)
    {
        Serilog.Log.Information("User.Identity.IsAuthenticated: {IsAuthenticated}", user.Identity.IsAuthenticated);
        Serilog.Log.Information("User.Identity.Name: {Name}", user.Identity.Name);
        foreach (var claim in user.Claims)
        {
            Serilog.Log.Information("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }
    }
    else
    {
        Serilog.Log.Information("No user identity present on request.");
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers for auth endpoints
app.MapControllers();

app.MapGraphQL("/graphql");

// To enable the Banana Cake Pop IDE, add the appropriate HotChocolate tooling package
// and call `app.UseBananaCakePop()` here (optional).

app.Run();

