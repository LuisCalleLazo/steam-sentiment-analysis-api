using Microsoft.AspNetCore.Mvc;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Controllers;

[Route("api/comment")]
[ApiController]
public class CommentController(ICommentService service) : ControllerBase
{
  /// <summary>
  /// GET /api/comment/list?gameId={gameId}
  /// Scrapes Steam reviews for the given game, runs sentiment analysis,
  /// persists new comments to MongoDB, and returns the full list.
  /// </summary>
  [HttpGet("list")]
  public async Task<IActionResult> GetComments(string gameId)
  {
    try
    {
      var comments = await service.GetCommentsByGameId(gameId);
      return Ok(comments);
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }
}