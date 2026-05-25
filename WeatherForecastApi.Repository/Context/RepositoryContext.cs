using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using WeatherForecastApi.Common.Model;
using WeatherForecastApi.Common.SecretManager;

namespace WeatherForecastApi.Repository.Context;

public sealed class RepositoryContext
{
    private readonly IConfiguration _configuration;
    private readonly IOptionsSnapshot<DatabaseSecret> _secret;

    public RepositoryContext(IConfiguration configuration, IOptionsSnapshot<DatabaseSecret> secret)
    {
        _configuration = configuration;
        _secret = secret;
    }

    public NpgsqlConnection CreateConnection()
    {
        var connectionString = DbConnectionStringBuilder.Build(_configuration, _secret.Value);
        return new NpgsqlConnection(connectionString);
    }
}
