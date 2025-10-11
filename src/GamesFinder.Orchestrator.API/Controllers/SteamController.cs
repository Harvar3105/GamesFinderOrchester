using System;
using GamesFinder.Orchestrator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesFinder.Orchestrator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamController : ControllerBase
{
  private readonly ILogger<SteamController> _logger;
  private readonly SteamService _steamService;

  public SteamController(ILogger<SteamController> logger, SteamService steamService)
  {
    _logger = logger;
    _steamService = steamService;
  }

  [HttpPost("scrap")]
  [Authorize(Policy = "DevPolicy")]
  public async Task<IActionResult> ScrapSteamIdsAsync([FromBody] RequestModel model)
  {
    if (model.steamIds == null || model.steamIds.Count == 0)
    {
      return BadRequest("Steam ID list cannot be empty.");
    }

    try
    {
      var processedCount = await _steamService.ScrapIdsAsync(model.steamIds, model.updateExisting);
      return Ok(new { Message = $"Scraping task initiated for {processedCount} Steam IDs. Estimated time: {MathF.Ceiling(model.steamIds.Count / 200)} minutes" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error initiating scraping task for Steam IDs.");
      return StatusCode(500, "An error occurred while processing your request.");
    }
  }

  public sealed record RequestModel
  {
    public List<int> steamIds { get; init; } = new();
    public bool updateExisting { get; init; } = false;
  }
}
