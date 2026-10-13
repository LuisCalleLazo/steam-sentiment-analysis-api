using SteamSentimentAnalysis.DTOs;

namespace SteamSentimentAnalysis.Services.Interfaces;

public interface ISentimentAnalysis
{
  Task<bool> GetAnalysisSentimentByComment(string comment);
  Task<bool> GetAnalysisSentimentByComment(List<string> comment);
  Task<HistogramResultDto> GetHistogramByGameId(
    string gameId, 
    DateTime? startDate, 
    DateTime? endDate);
  Task<List<GameDto>> GetRankedGames();
  Task<List<CommentTop>> GetCommentsTop(
    DateTime? startDate, 
    DateTime? endDate);
}