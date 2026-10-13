namespace SteamSentimentAnalysis.Entities;

public class CommentEntity
{
  public string Id { get; set; } = null!;
  public string Comment { get; set; } = null!;
  public bool IsBad { get; set; }
  public string SteamGameTitle { get; set; } = null!;
  public string Sentence { get; set; } = null!;
  public double HoursGameplay { get; set; }
}