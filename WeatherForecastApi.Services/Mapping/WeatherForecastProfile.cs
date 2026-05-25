using AutoMapper;
using WeatherForecastApi.Common.Entities;
using WeatherForecastApi.Repository.Model.Dto;

namespace WeatherForecastApi.Services.Mapping;

public sealed class WeatherForecastProfile : Profile
{
    public WeatherForecastProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();

        CreateMap<CreateWeatherForecastRequest, WeatherForecast>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateWeatherForecastRequest, WeatherForecast>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());
    }
}
