namespace WeatherForecastApi.Repository.Model.Dto;

public sealed record WeatherForecastDto(
    int Id,
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary,
    string? Location,
    DateTimeOffset CreatedAt);
