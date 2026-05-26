using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using WeatherForecastApi.Common.Model;
using WeatherForecastApi.Common.SecretManager;
using WeatherForecastApi.Peer.Middleware;
using WeatherForecastApi.Repository.DependencyInjection;
using WeatherForecastApi.Services.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var keyVaultOptions = builder.Configuration
    .GetSection(KeyVaultOptions.SectionName)
    .Get<KeyVaultOptions>() ?? new KeyVaultOptions();

if (string.IsNullOrWhiteSpace(keyVaultOptions.VaultUri))
{
    throw new InvalidOperationException("KeyVault:VaultUri is required.");
}

if (string.IsNullOrWhiteSpace(keyVaultOptions.DbSecretName))
{
    throw new InvalidOperationException("KeyVault:DbSecretName is required.");
}

try
{
    builder.Configuration.AddAzureKeyVault(keyVaultOptions, TimeSpan.FromHours(1));
}
catch (Exception ex)
{
    var bootstrapLogger = LoggerFactory
        .Create(b => b.AddConsole())
        .CreateLogger("KeyVault");
    bootstrapLogger.LogError(ex, "Failed to load Azure Key Vault secrets from {VaultUri}", keyVaultOptions.VaultUri);
    throw;
}

try
{
    var dbSecret = await AzureKeyVaultSecretManager.GetDatabaseSecretAsync(
        keyVaultOptions.VaultUri,
        keyVaultOptions.DbSecretName);

    builder.Services.Configure<DatabaseSecret>(o =>
    {
        o.Username = dbSecret.Username;
        o.Password = dbSecret.Password;
    });
}
catch (Exception ex)
{
    var bootstrapLogger = LoggerFactory
        .Create(b => b.AddConsole())
        .CreateLogger("KeyVault");
    bootstrapLogger.LogError(ex, "Failed to load DatabaseSecret '{Name}' from Key Vault", keyVaultOptions.DbSecretName);
    throw;
}

builder.Services.Configure<KeyVaultOptions>(builder.Configuration.GetSection(KeyVaultOptions.SectionName));

builder.Services.AddWeatherForecastRepository();
builder.Services.AddWeatherForecastServices();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(WeatherForecastApi.Peer.Controllers.WeatherForecastsController).Assembly);

if (builder.Environment.IsDevelopment())
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
    });
}

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather Forecast API",
        Version = "v1",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<WeatherForecastNotFoundExceptionMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
