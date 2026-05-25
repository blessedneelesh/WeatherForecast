using FluentAssertions;
using WeatherForecastApi.Common.Entities;
using WeatherForecastApi.Repository.Repositories;
using Xunit;

namespace WeatherForecastApi.Repository.Tests;

[Collection(PostgresCollection.Name)]
public sealed class WeatherForecastRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly WeatherForecastRepository _repository;

    public WeatherForecastRepositoryTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
        _repository = new WeatherForecastRepository(fixture.Context);
    }

    public Task InitializeAsync() => _fixture.TruncateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_persists_and_returns_new_id()
    {
        var forecast = new WeatherForecast
        {
            Date = new DateOnly(2026, 5, 25),
            TemperatureC = 22,
            Summary = "Pleasant",
            Location = "Raleigh",
        };

        var id = await _repository.CreateAsync(forecast);

        id.Should().BeGreaterThan(0);

        var loaded = await _repository.GetByIdAsync(id);
        loaded.Should().NotBeNull();
        loaded!.Summary.Should().Be("Pleasant");
        loaded.Location.Should().Be("Raleigh");
        loaded.TemperatureC.Should().Be(22);
    }

    [Fact]
    public async Task GetAllAsync_returns_all_rows()
    {
        await _repository.CreateAsync(new WeatherForecast { Date = new DateOnly(2026, 5, 25), TemperatureC = 22, Summary = "A" });
        await _repository.CreateAsync(new WeatherForecast { Date = new DateOnly(2026, 5, 26), TemperatureC = 18, Summary = "B" });

        var all = await _repository.GetAllAsync();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_modifies_existing_row()
    {
        var id = await _repository.CreateAsync(new WeatherForecast
        {
            Date = new DateOnly(2026, 5, 25),
            TemperatureC = 22,
            Summary = "Old",
        });

        var updated = await _repository.UpdateAsync(new WeatherForecast
        {
            Id = id,
            Date = new DateOnly(2026, 5, 25),
            TemperatureC = 30,
            Summary = "New",
            Location = "Atlanta",
        });

        updated.Should().BeTrue();
        var loaded = await _repository.GetByIdAsync(id);
        loaded!.TemperatureC.Should().Be(30);
        loaded.Summary.Should().Be("New");
        loaded.Location.Should().Be("Atlanta");
    }

    [Fact]
    public async Task UpdateAsync_returns_false_when_row_missing()
    {
        var updated = await _repository.UpdateAsync(new WeatherForecast
        {
            Id = 9999,
            Date = new DateOnly(2026, 5, 25),
            TemperatureC = 10,
        });

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_removes_row()
    {
        var id = await _repository.CreateAsync(new WeatherForecast
        {
            Date = new DateOnly(2026, 5, 25),
            TemperatureC = 10,
        });

        var deleted = await _repository.DeleteAsync(id);

        deleted.Should().BeTrue();
        (await _repository.GetByIdAsync(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_row_missing()
    {
        (await _repository.DeleteAsync(9999)).Should().BeFalse();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres collection";
}
