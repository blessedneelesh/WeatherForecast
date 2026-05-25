namespace WeatherForecastApi.Common.Exceptions;

public sealed class WeatherForecastNotFoundException : Exception
{
    public WeatherForecastNotFoundException(int id)
        : base($"Weather forecast with id {id} was not found.")
    {
        Id = id;
    }

    public int Id { get; }
}
