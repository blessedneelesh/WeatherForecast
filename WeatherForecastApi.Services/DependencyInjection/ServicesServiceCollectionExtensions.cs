using Microsoft.Extensions.DependencyInjection;
using WeatherForecastApi.Services.Abstractions.ServiceInterfaces;
using WeatherForecastApi.Services.Mapping;

namespace WeatherForecastApi.Services.DependencyInjection;

public static class ServicesServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherForecastServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<WeatherForecastProfile>());
        services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        return services;
    }
}
