using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherForecastApi.Common.Entities;
using WeatherForecastApi.Common.Exceptions;
using WeatherForecastApi.Repository.Interface;
using WeatherForecastApi.Repository.Model.Dto;
using WeatherForecastApi.Services;
using WeatherForecastApi.Services.Mapping;
using Xunit;

namespace WeatherForecastApi.Tests;

public sealed class WeatherForecastServiceTests
{
    private readonly Mock<IWeatherForecastRepository> _repository = new(MockBehavior.Strict);
    private readonly IMapper _mapper;
    private readonly WeatherForecastService _sut;

    public WeatherForecastServiceTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<WeatherForecastProfile>(),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _sut = new WeatherForecastService(_repository.Object, _mapper, NullLogger<WeatherForecastService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_returns_mapped_dtos()
    {
        var entities = new List<WeatherForecast>
        {
            new() { Id = 1, Date = new DateOnly(2026, 5, 25), TemperatureC = 20, Summary = "Mild" },
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].TemperatureC.Should().Be(20);
        result[0].TemperatureF.Should().Be(67);
    }

    [Fact]
    public async Task GetByIdAsync_throws_when_repository_returns_null()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((WeatherForecast?)null);

        await FluentActions.Awaiting(() => _sut.GetByIdAsync(99))
            .Should().ThrowAsync<WeatherForecastNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_persists_and_returns_dto()
    {
        var request = new CreateWeatherForecastRequest
        {
            Date = new DateOnly(2026, 6, 1),
            TemperatureC = 15,
            Summary = "Cool",
            Location = "Raleigh",
        };

        _repository
            .Setup(r => r.CreateAsync(It.IsAny<WeatherForecast>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);
        _repository
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherForecast
            {
                Id = 42,
                Date = request.Date,
                TemperatureC = request.TemperatureC,
                Summary = request.Summary,
                Location = request.Location,
                CreatedAt = DateTimeOffset.UtcNow,
            });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(42);
        result.Location.Should().Be("Raleigh");
    }

    [Fact]
    public async Task UpdateAsync_throws_when_no_rows_affected()
    {
        _repository
            .Setup(r => r.UpdateAsync(It.IsAny<WeatherForecast>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new UpdateWeatherForecastRequest { Date = new DateOnly(2026, 5, 1), TemperatureC = 1 };

        await FluentActions.Awaiting(() => _sut.UpdateAsync(7, request))
            .Should().ThrowAsync<WeatherForecastNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_throws_when_no_rows_affected()
    {
        _repository.Setup(r => r.DeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions.Awaiting(() => _sut.DeleteAsync(7))
            .Should().ThrowAsync<WeatherForecastNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_succeeds_when_row_deleted()
    {
        _repository.Setup(r => r.DeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.DeleteAsync(7);

        _repository.Verify(r => r.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}
