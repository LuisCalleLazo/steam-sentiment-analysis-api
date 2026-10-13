namespace SteamSentimentAnalysis.DTOs;

public class HistogramResultDto
{
  public string Title { get; set; } = string.Empty;
  public int TotalCount { get; set; }
  public HistogramSummaryDto? Summary { get; set; }
  public List<HistogramBinDto> Bins { get; set; } = new();
}

public class HistogramSummaryDto
{
  public double Mean { get; set; }
  public double Median { get; set; }
  public double StandardDeviation { get; set; }
}

public class HistogramBinDto
{
  public double LowerLimit { get; set; }
  public double UpperLimit { get; set; }
  public string BinLabel { get; set; } = string.Empty;
  public int Frequency { get; set; }
  public double RelativeFrequency { get; set; }
}
