using SteamSentimentAnalysis.DTOs;

namespace SteamSentimentAnalysis.Services.Interfaces;

public interface ICommentService
{
  Task<List<CommentDto>> GetCommentsByGameId(string gameId);
}