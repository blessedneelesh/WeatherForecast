using Dapper;
using WeatherForecastApi.Common.Entities;
using WeatherForecastApi.Repository.Context;
using WeatherForecastApi.Repository.Interface;

namespace WeatherForecastApi.Repository.Repositories;

public sealed class WeatherForecastRepository : IWeatherForecastRepository
{
    private const string SelectColumns =
        "id AS Id, date AS Date, temperature_c AS TemperatureC, summary AS Summary, location AS Location, created_at AS CreatedAt";

    private readonly RepositoryContext _context;

    public WeatherForecastRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = $"SELECT {SelectColumns} FROM weather_forecasts ORDER BY date DESC, id DESC;";

        await using var connection = _context.CreateConnection();
        var rows = await connection.QueryAsync<WeatherForecast>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<WeatherForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = $"SELECT {SelectColumns} FROM weather_forecasts WHERE id = @Id;";

        await using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<WeatherForecast>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO weather_forecasts (date, temperature_c, summary, location)
            VALUES (@Date, @TemperatureC, @Summary, @Location)
            RETURNING id;";

        await using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new
            {
                forecast.Date,
                forecast.TemperatureC,
                forecast.Summary,
                forecast.Location,
            }, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE weather_forecasts
            SET date = @Date,
                temperature_c = @TemperatureC,
                summary = @Summary,
                location = @Location
            WHERE id = @Id;";

        await using var connection = _context.CreateConnection();
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                forecast.Id,
                forecast.Date,
                forecast.TemperatureC,
                forecast.Summary,
                forecast.Location,
            }, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM weather_forecasts WHERE id = @Id;";

        await using var connection = _context.CreateConnection();
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}
