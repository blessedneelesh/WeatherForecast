using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeatherForecastApi.Repository.Model.Dto;
using WeatherForecastApi.Services.Abstractions.ServiceInterfaces;

namespace WeatherForecastApi.Peer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WeatherForecastsController : ControllerBase
{
    private readonly IWeatherForecastService _service;

    public WeatherForecastsController(IWeatherForecastService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WeatherForecastDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WeatherForecastDto>>> GetAll(CancellationToken cancellationToken)
    {
        var forecasts = await _service.GetAllAsync(cancellationToken);
        return Ok(forecasts);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherForecastDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var forecast = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(forecast);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherForecastDto>> Create(
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherForecastDto>> Update(
        int id,
        [FromBody] UpdateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
