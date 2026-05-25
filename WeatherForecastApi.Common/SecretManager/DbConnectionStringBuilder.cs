using Microsoft.Extensions.Configuration;
using Npgsql;
using WeatherForecastApi.Common.Model;

namespace WeatherForecastApi.Common.SecretManager;

public static class DbConnectionStringBuilder
{
    public static string Build(IConfiguration configuration, DatabaseSecret secret)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(secret);

        var host = configuration["ConnectionStrings:Host"];
        var database = configuration["ConnectionStrings:Database"];
        var portRaw = configuration["ConnectionStrings:Port"];
        var schema = configuration["ConnectionStrings:Schema"];
        var sslModeRaw = configuration["ConnectionStrings:SslMode"];

        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("ConnectionStrings:Host is required.");
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("ConnectionStrings:Database is required.");
        }

        if (string.IsNullOrWhiteSpace(secret.Username))
        {
            throw new InvalidOperationException("DatabaseSecret.Username is required.");
        }

        if (string.IsNullOrWhiteSpace(secret.Password))
        {
            throw new InvalidOperationException("DatabaseSecret.Password is required.");
        }

        if (!int.TryParse(portRaw, out var port))
        {
            port = 5432;
        }

        if (!Enum.TryParse<SslMode>(sslModeRaw, ignoreCase: true, out var sslMode))
        {
            sslMode = SslMode.Require;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = secret.Username,
            Password = secret.Password,
            SslMode = sslMode,
            SearchPath = string.IsNullOrWhiteSpace(schema) ? "public" : schema,
        };

        return builder.ConnectionString;
    }
}
