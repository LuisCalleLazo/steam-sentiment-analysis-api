using SteamSentimentAnalysis.DTOs;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Services;

public class CommentService : ICommentService
{
  public Task<List<CommentDto>> GetCommentsByGameId(string gameId)
  {
    throw new NotImplementedException();
  }
}