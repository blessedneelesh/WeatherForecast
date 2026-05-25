using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeatherForecastApi.Common.Exceptions;

namespace WeatherForecastApi.Peer.Middleware;

public sealed class WeatherForecastNotFoundExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public WeatherForecastNotFoundExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (WeatherForecastNotFoundException ex)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Weather forecast not found",
                Detail = ex.Message,
                Instance = context.Request.Path,
            };

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
