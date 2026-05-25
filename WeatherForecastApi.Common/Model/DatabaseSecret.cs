using Newtonsoft.Json;

namespace WeatherForecastApi.Common.Model;

public sealed class DatabaseSecret
{
    [JsonProperty("username")]
    public string Username { get; set; } = "";

    [JsonProperty("password")]
    public string Password { get; set; } = "";
}
