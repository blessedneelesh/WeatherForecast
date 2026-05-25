using WeatherForecastApi.Common.Entities;

namespace WeatherForecastApi.Repository.Interface;

public interface IWeatherForecastRepository
{
    Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WeatherForecast?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
