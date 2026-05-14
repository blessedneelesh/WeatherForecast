using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WeatherForecast; // Add this using directive to reference the Program class

namespace WeatherForecast.Tests
{
    public class WeatherForecastControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public WeatherForecastControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Add_TwoPositiveNumbers_ReturnsCorrectSum()
        {
            // Arrange
            int a = 5;
            int b = 10;

            // Act
            var response = await _client.GetAsync($"/WeatherForecast/add?a={a}&b={b}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(15, result);
        }

        [Fact]
        public async Task Add_TwoNegativeNumbers_ReturnsCorrectSum()
        {
            // Arrange
            int a = -5;
            int b = -10;

            // Act
            var response = await _client.GetAsync($"/WeatherForecast/add?a={a}&b={b}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(-15, result);
        }

        [Fact]
        public async Task Add_PositiveAndNegativeNumbers_ReturnsCorrectSum()
        {
            // Arrange
            int a = 10;
            int b = -5;

            // Act
            var response = await _client.GetAsync($"/WeatherForecast/add?a={a}&b={b}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task Add_WithZero_ReturnsCorrectSum()
        {
            // Arrange
            int a = 0;
            int b = 10;

            // Act
            var response = await _client.GetAsync($"/WeatherForecast/add?a={a}&b={b}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task Add_LargeNumbers_ReturnsCorrectSum()
        {
            // Arrange
            int a = 1000000;
            int b = 2000000;

            // Act
            var response = await _client.GetAsync($"/WeatherForecast/add?a={a}&b={b}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(3000000, result);
        }

        [Fact]
        public async Task Add_ReturnsOkStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/WeatherForecast/add?a=1&b=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}