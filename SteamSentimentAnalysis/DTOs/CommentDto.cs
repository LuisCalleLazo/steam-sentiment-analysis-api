namespace SteamSentimentAnalysis.DTOs;

public class CommentDto
{
  public string Comment { get; set; } = null!;
  public DateTime Date { get; set; }
  public double HoursGameplay { get; set; }
}

public class CommentTop
{
  public string Comment { get; set; } = null!;
  public int Count { get; set; }
  public bool IsBad { get; set; }
}