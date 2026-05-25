using System.ComponentModel.DataAnnotations;

namespace WeatherForecastApi.Repository.Model.Dto;

public sealed class CreateWeatherForecastRequest
{
    [Required]
    public DateOnly Date { get; init; }

    [Range(-100, 100)]
    public int TemperatureC { get; init; }

    [StringLength(256)]
    public string? Summary { get; init; }

    [StringLength(128)]
    public string? Location { get; init; }
}
