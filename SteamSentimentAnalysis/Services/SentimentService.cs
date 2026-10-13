using SteamSentimentAnalysis.DTOs;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Services;

public class SentimentService : ISentimentAnalysis
{
  public Task<bool> GetAnalysisSentimentByComment(string comment)
  {
    throw new NotImplementedException();
  }

  public Task<bool> GetAnalysisSentimentByComment(List<string> comment)
  {
    throw new NotImplementedException();
  }

  public Task<HistogramResultDto> GetHistogramByGameId(string gameId, DateTime? startDate, DateTime? endDate)
  {
    throw new NotImplementedException();
  }

  public Task<List<GameDto>> GetRankedGames()
  {
    throw new NotImplementedException();
  }

  public Task<List<CommentTop>> GetCommentsTop(DateTime? startDate, DateTime? endDate)
  {
    throw new NotImplementedException();
  }
}