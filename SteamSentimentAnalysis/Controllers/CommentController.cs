using Microsoft.AspNetCore.Mvc;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Controllers;

[Route("api/comment")]
[ApiController]
public class CommentController(
  ICommentService service) : ControllerBase
{
  [HttpGet]
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