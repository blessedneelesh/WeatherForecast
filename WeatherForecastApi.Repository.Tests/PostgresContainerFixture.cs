using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;
using WeatherForecastApi.Common.Model;
using WeatherForecastApi.Repository.Context;
using WeatherForecastApi.Repository.TypeHandlers;
using Xunit;

namespace WeatherForecastApi.Repository.Tests;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private const string DatabaseName = "weatherforecast_test";
    private const string Username = "postgres";
    private const string Password = "postgres";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        .Build();

    public RepositoryContext Context { get; private set; } = default!;
    private string _adminConnectionString = string.Empty;

    public async Task InitializeAsync()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(5432).ToString();

        _adminConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = _container.Hostname,
            Port = int.Parse(port),
            Database = DatabaseName,
            Username = Username,
            Password = Password,
            SslMode = SslMode.Disable,
            SearchPath = "public",
        }.ConnectionString;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Host"] = _container.Hostname,
                ["ConnectionStrings:Port"] = port,
                ["ConnectionStrings:Database"] = DatabaseName,
                ["ConnectionStrings:Schema"] = "public",
                ["ConnectionStrings:SslMode"] = "Disable",
            })
            .Build();

        var secret = new DatabaseSecret { Username = Username, Password = Password };
        Context = new RepositoryContext(configuration, new StaticOptionsSnapshot<DatabaseSecret>(secret));

        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Sql_Scripts", "Up", "V1_0_0_0__WeatherForecasts_Schema.sql");
        var schema = await File.ReadAllTextAsync(schemaPath);

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var cmd = new NpgsqlCommand(schema, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task TruncateAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.ExecuteAsync("TRUNCATE TABLE weather_forecasts RESTART IDENTITY;");
    }

    private sealed class StaticOptionsSnapshot<T> : IOptionsSnapshot<T> where T : class
    {
        public StaticOptionsSnapshot(T value) => Value = value;

        public T Value { get; }

        public T Get(string? name) => Value;
    }
}
