using SteamSentimentAnalysis.DTOs;
using SteamSentimentAnalysis.Repositories;
using SteamSentimentAnalysis.Services.Interfaces;
using SentimentAnalyzer.Onnx.Multilingual;

namespace SteamSentimentAnalysis.Services;

public class SentimentService(
  ICommentRepository commentRepository
  ) : ISentimentAnalysis
{
  // Analizador multilingüe (soporta español) creado una sola vez
  private static readonly Lazy<SentimentAnalyzer.SentimentAnalyzer> Analyzer =
    new(() => SentimentAnalyzer.SentimentAnalyzer.CreateBuilder()
      .UseDistilBert()
      .Build());

  // ────────────────────────────── Sentimiento individual ──────────────────────────────
  public Task<bool> GetAnalysisSentimentByComment(string comment)
  {
    if (string.IsNullOrWhiteSpace(comment))
      return Task.FromResult(false);

    var result = Analyzer.Value.Analyze(comment);
    
    // Usar directamente la propiedad IsPositive (recomendado)
    return Task.FromResult(result.IsPositive);
  }

  // Devuelve true si la mayoría de la lista es positiva
  public async Task<bool> GetAnalysisSentimentByComment(List<string>? comments)
  {
    if (comments is null || comments.Count == 0)
      return false;

    var results = await Task.WhenAll(
      comments.Select(GetAnalysisSentimentByComment));

    return results.Count(r => r) >= results.Length / 2.0;
  }

  // ────────────────────────────── Histograma por juego ──────────────────────────────
  public async Task<HistogramResultDto> GetHistogramByGameId(
    string gameId, DateTime? startDate, DateTime? endDate)
  {
    var comments = await commentRepository.GetByGameIdAsync(gameId);

    // Filtrado por fecha (ajusta según si Comment tiene propiedad Date)
    // var filtered = comments.Where(c =>
    //   (!startDate.HasValue || c.Date >= startDate) &&
    //   (!endDate.HasValue   || c.Date <= endDate)).ToList();
    var filtered = comments.ToList();

    if (filtered.Count == 0)
    {
      return new HistogramResultDto
      {
        Title = $"Histograma de horas jugadas - {gameId}",
        TotalCount = 0
      };
    }

    // Histograma sobre HoursGameplay (esto es lo que suele representar tu HistogramBinDto)
    var hours = filtered.Select(c => c.HoursGameplay).ToList();
    var bins = BuildHistogram(hours);

    return new HistogramResultDto
    {
      Title = $"Histograma de horas jugadas - {filtered[0].GameTitle}",
      TotalCount = hours.Count,
      Summary = new HistogramSummaryDto
      {
        Mean = hours.Average(),
        Median = CalculateMedian(hours),
        StandardDeviation = CalculateStandardDeviation(hours)
      },
      Bins = bins
    };
  }

  // ────────────────────────────── Ranking de juegos ──────────────────────────────
  public async Task<List<GameDto>> GetRankedGames()
  {
    var allComments = await commentRepository.GetAllAsync();

    return allComments
      .GroupBy(c => c.GameId)
      .Select(g => new
      {
        GameId = g.Key,
        Title = g.First().GameTitle,
        Total = g.Count(),
        Positives = g.Count(c => c.IsGood)
      })
      .OrderByDescending(g => g.Positives / (double)g.Total)
      .Select(g => new GameDto
      {
        Id = g.GameId,
        Title = g.Title,
        Description = string.Empty,
        Date = default,
        Developer = string.Empty,
        Category = string.Empty,
        ImageUrl = string.Empty
      })
      .ToList();
  }

  // ────────────────────────────── Top comentarios ──────────────────────────────
  public async Task<List<CommentTop>> GetCommentsTop(
    DateTime? startDate, DateTime? endDate)
  {
    var comments = await commentRepository.GetAllAsync();

    // Si Comment tuviera fecha, aquí filtrarías:
    // comments = comments.Where(c => ...);

    return comments
      .GroupBy(c => c.Content)
      .Select(g => new CommentTop
      {
        Comment = g.Key,
        Count = g.Count(),
        IsGood = g.Any(c => c.IsGood),
        IsBad = g.Any(c => c.IsBad)
      })
      .OrderByDescending(c => c.Count)
      .Take(10)
      .ToList();
  }

  // ────────────────────────────── Helpers ──────────────────────────────
  private static List<HistogramBinDto> BuildHistogram(
    List<double> values, int binCount = 10)
  {
    var min = values.Min();
    var max = values.Max();

    if (Math.Abs(max - min) < double.Epsilon)
    {
      return
      [
        new HistogramBinDto
        {
          LowerLimit = min,
          UpperLimit = max,
          BinLabel = $"{min:F1}",
          Frequency = values.Count,
          RelativeFrequency = 1.0
        }
      ];
    }

    var width = (max - min) / binCount;
    var bins = new List<HistogramBinDto>(binCount);

    for (var i = 0; i < binCount; i++)
    {
      var lower = min + i * width;
      var upper = lower + width;

      // Último bin incluye el extremo superior
      var freq = (i == binCount - 1)
        ? values.Count(v => v >= lower && v <= upper)
        : values.Count(v => v >= lower && v < upper);

      bins.Add(new HistogramBinDto
      {
        LowerLimit = lower,
        UpperLimit = upper,
        BinLabel = $"{lower:F1} - {upper:F1}",
        Frequency = freq,
        RelativeFrequency = freq / (double)values.Count
      });
    }

    return bins;
  }

  private static double CalculateMedian(List<double> values)
  {
    var sorted = values.OrderBy(v => v).ToList();
    var mid = sorted.Count / 2;
    return sorted.Count % 2 == 0
      ? (sorted[mid - 1] + sorted[mid]) / 2.0
      : sorted[mid];
  }

  private static double CalculateStandardDeviation(List<double> values)
  {
    if (values.Count < 2) return 0;
    var mean = values.Average();
    var sumSq = values.Sum(v => Math.Pow(v - mean, 2));
    return Math.Sqrt(sumSq / (values.Count - 1));
  }
}