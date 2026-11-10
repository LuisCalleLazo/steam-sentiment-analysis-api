using Microsoft.AspNetCore.Mvc;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Controllers;

[Route("api/sentiment")]
[ApiController]
public class SentimentController(ISentimentAnalysis service) : ControllerBase
{
  /// <summary>
  /// GET /api/sentiment/histogram?gameId={gameId}&amp;startDate={date}&amp;endDate={date}
  /// Returns a pre-computed histogram of hours played distribution for a game,
  /// optionally filtered by date range.
  /// </summary>
  [HttpGet("histogram")]
  public async Task<IActionResult> GetHistogram(
    string gameId,
    DateTime? startDate = null,
    DateTime? endDate = null)
  {
    try
    {
      var result = await service.GetHistogramByGameId(gameId, startDate, endDate);
      return Ok(result);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }

  /// <summary>
  /// GET /api/sentiment/ranked
  /// Returns all games sorted by their positive review ratio (descending).
  /// Uses pre-computed IsGood/IsBad flags stored in MongoDB.
  /// </summary>
  [HttpGet("ranked")]
  public async Task<IActionResult> GetRankedGames()
  {
    try
    {
      var result = await service.GetRankedGames();
      return Ok(result);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }

  /// <summary>
  /// GET /api/sentiment/top?startDate={date}&amp;endDate={date}
  /// Returns the top 10 most-mentioned comments globally,
  /// with their IsGood/IsBad flags. Date filter is optional.
  /// </summary>
  [HttpGet("top")]
  public async Task<IActionResult> GetTopComments(
    DateTime? startDate = null,
    DateTime? endDate = null)
  {
    try
    {
      var result = await service.GetCommentsTop(startDate, endDate);
      return Ok(result);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }
}