using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Services;

public class ReportService : IReportService
{
  public Task<byte[]> GetGameReportPdf(string gameId, DateTime? startDate, DateTime? endDate)
  {
    throw new NotImplementedException();
  }
}