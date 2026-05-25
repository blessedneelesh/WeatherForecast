using Dapper;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecastApi.Repository.Context;
using WeatherForecastApi.Repository.Interface;
using WeatherForecastApi.Repository.Repositories;
using WeatherForecastApi.Repository.TypeHandlers;

namespace WeatherForecastApi.Repository.DependencyInjection;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherForecastRepository(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddScoped<RepositoryContext>();
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

        return services;
    }
}
