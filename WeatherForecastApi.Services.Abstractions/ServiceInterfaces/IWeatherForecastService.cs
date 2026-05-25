using WeatherForecastApi.Repository.Model.Dto;

namespace WeatherForecastApi.Services.Abstractions.ServiceInterfaces;

public interface IWeatherForecastService
{
    Task<IReadOnlyList<WeatherForecastDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WeatherForecastDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WeatherForecastDto> CreateAsync(CreateWeatherForecastRequest request, CancellationToken cancellationToken = default);
    Task<WeatherForecastDto> UpdateAsync(int id, UpdateWeatherForecastRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
