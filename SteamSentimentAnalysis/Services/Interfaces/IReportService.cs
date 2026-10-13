namespace SteamSentimentAnalysis.Services.Interfaces;

public interface IReportService
{
  Task<byte[]> GetGameReportPdf(
    string gameId, 
    DateTime? startDate, 
    DateTime? endDate);
}