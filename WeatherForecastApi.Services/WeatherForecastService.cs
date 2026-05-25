using AutoMapper;
using Microsoft.Extensions.Logging;
using WeatherForecastApi.Common.Entities;
using WeatherForecastApi.Common.Exceptions;
using WeatherForecastApi.Repository.Interface;
using WeatherForecastApi.Repository.Model.Dto;
using WeatherForecastApi.Services.Abstractions.ServiceInterfaces;

namespace WeatherForecastApi.Services;

public sealed class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<WeatherForecastService> _logger;

    public WeatherForecastService(
        IWeatherForecastRepository repository,
        IMapper mapper,
        ILogger<WeatherForecastService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WeatherForecastDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var forecasts = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<WeatherForecastDto>>(forecasts);
    }

    public async Task<WeatherForecastDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var forecast = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new WeatherForecastNotFoundException(id);

        return _mapper.Map<WeatherForecastDto>(forecast);
    }

    public async Task<WeatherForecastDto> CreateAsync(CreateWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<WeatherForecast>(request);

        var newId = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created weather forecast {Id}", newId);

        var created = await _repository.GetByIdAsync(newId, cancellationToken)
            ?? throw new InvalidOperationException($"Failed to load newly created forecast {newId}.");

        return _mapper.Map<WeatherForecastDto>(created);
    }

    public async Task<WeatherForecastDto> UpdateAsync(int id, UpdateWeatherForecastRequest request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<WeatherForecast>(request);
        entity.Id = id;

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        if (!updated)
        {
            throw new WeatherForecastNotFoundException(id);
        }

        var refreshed = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new WeatherForecastNotFoundException(id);

        return _mapper.Map<WeatherForecastDto>(refreshed);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new WeatherForecastNotFoundException(id);
        }

        _logger.LogInformation("Deleted weather forecast {Id}", id);
    }
}
