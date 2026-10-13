namespace SteamSentimentAnalysis.DTOs;

public class GameDto
{
  public string Id { get; set; } = null!;
  public string Title { get; set; } = null!;
  public string Description { get; set; } = null!;
  public DateTime Date { get; set; }
  public string Developer { get; set; } = null!;
  public string Category { get; set; } = null!;
  public string ImageUrl { get; set; } = null!;
}