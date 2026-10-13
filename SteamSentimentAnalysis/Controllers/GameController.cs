using Microsoft.AspNetCore.Mvc;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Controllers;


[Route("api/game")]
[ApiController]
public class GameController(IGameService service) : ControllerBase
{
  [HttpGet("list")]
  public async Task<IActionResult> GetGames(string search)
  {
    try
    {
      var games = await service.GetGamesOfSteam(search);
      return Ok(games);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }
  
  [HttpGet]
  public async Task<IActionResult> GetGameDetail(string gameId)
  {
    try
    {
      var game = await service.GetGameDetail(gameId);
      return Ok(game);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }
}