namespace SteamSentimentAnalysis.DTOs;

public class CommentDto
{
  public string Comment { get; set; } = null!;
  public double HoursGameplay { get; set; }
  /// <summary>
  /// Sentimiento: 1 = positivo, -1 = negativo, 0 = neutral.
  /// Derivado de IsGood/IsBad del modelo de análisis ML.
  /// </summary>
  public int Sentiment { get; set; }
  public DateTime Date { get; set; }
}

public class CommentTop
{
  public string Comment { get; set; } = null!;
  public int Count { get; set; }
  public bool IsBad { get; set; }
  public bool IsGood { get; set; }
}