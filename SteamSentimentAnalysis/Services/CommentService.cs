using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SteamSentimentAnalysis.DTOs;
using SteamSentimentAnalysis.Entities;
using SteamSentimentAnalysis.Repositories;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Services;

public partial class CommentService(
  HttpClient httpClient,
  ICommentRepository commentRepository,
  ISentimentAnalysis sentimentService) : ICommentService
{
  private const int MaxPages = 30;
  private const int TargetComments = 50; // objetivo mínimo

  public async Task<List<CommentDto>> GetCommentsByGameId(string gameId)
  {
    var scraped = new Dictionary<string, RawComment>();

    for (var page = 1; page <= MaxPages; page++)
    {
      var pageComments = await ScrapePage(gameId, page);
      if (pageComments.Count == 0)
        break;

      var added = pageComments.Count(c => scraped.TryAdd(c.Id, c));

      // Solo paramos si Steam repite TODO el contenido de la página
      if (added == 0)
        break;

      // Si ya tenemos suficientes, paramos de scrapear
      if (scraped.Count >= TargetComments)
        break;

      await Task.Delay(500);
    }

    if (scraped.Count == 0)
      return [];

    // Filtrar los que ya existen en BD ANTES de analizar sentimiento
    var existing = await commentRepository.GetByGameIdAsync(gameId);
    var existingIds = existing.Select(c => c.Id).ToHashSet();

    var toInsert = scraped.Values
      .Where(c => !existingIds.Contains(c.Id))
      .ToList();

    // Analizar sentimiento SOLO de los nuevos (en paralelo)
    if (toInsert.Count > 0)
    {
      var sentimentTasks = toInsert
        .Select(c => sentimentService.GetAnalysisSentimentByComment(c.Content))
        .ToArray();

      var sentimentResults = await Task.WhenAll(sentimentTasks);

      var entities = new List<Comment>(toInsert.Count);
      for (var i = 0; i < toInsert.Count; i++)
      {
        var raw = toInsert[i];
        var isGood = sentimentResults[i];

        entities.Add(new Comment
        {
          Id = raw.Id,
          Content = raw.Content,
          HoursGameplay = raw.Hours,
          IsGood = isGood,
          IsBad = !isGood,
          GameId = gameId,
          GameTitle = raw.GameTitle
        });
      }

      await commentRepository.CreateManyAsync(entities);
    }

    // Devolver TODOS los comentarios únicos scrapeados (nuevos + ya existentes en esta sesión)
    return scraped.Values
      .Select(c => new CommentDto
      {
        Comment = c.Content,
        HoursGameplay = c.Hours
      })
      .ToList();
  }

  private async Task<List<RawComment>> ScrapePage(string gameId, int page)
  {
    // Quitamos filterLanguage para traer TODAS las reseñas (no solo español).
    // Si quieres solo español, deja filterLanguage=spanish pero entonces
    // el número de comentarios dependerá de cuántos haya en ese idioma.
    var url =
      $"https://steamcommunity.com/app/{Uri.EscapeDataString(gameId)}/reviews/" +
      $"?p={page}&browsefilter=mostrecent&l=spanish";

    using var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.UserAgent.ParseAdd(
      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
      "AppleWebKit/537.36 (KHTML, like Gecko) " +
      "Chrome/130.0.0.0 Safari/537.36");
    request.Headers.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");
    request.Headers.Add(
      "Cookie",
      "birthtime=568022401; lastagecheckage=1-0-1988; mature_content=1");

    using var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var html = await response.Content.ReadAsStringAsync();

    var document = new HtmlDocument();
    document.LoadHtml(html);

    var cards = document.DocumentNode.SelectNodes(
      $"//div[@id='page{page}']//div[contains(@class,'apphub_CardContentMain')]");

    if (cards is null)
      return [];

    var gameTitle = HtmlEntity.DeEntitize(
      document.DocumentNode.SelectSingleNode(
        "//div[contains(@class,'apphub_AppName')]")?.InnerText ?? "").Trim();

    var result = new List<RawComment>();

    foreach (var card in cards)
    {
      var content = ExtractContent(card);
      if (string.IsNullOrWhiteSpace(content))
        continue;

      var hoursText = card.SelectSingleNode(".//div[contains(@class,'hours')]")?.InnerText;

      result.Add(new RawComment
      {
        Id = BuildId(gameId, content),
        Content = content,
        Hours = ParseHours(hoursText),
        GameTitle = gameTitle
      });
    }

    return result;
  }

  private static string ExtractContent(HtmlNode card)
  {
    var textNode = card.SelectSingleNode(
      ".//div[contains(@class,'apphub_CardTextContent')]");

    if (textNode is null)
      return string.Empty;

    // Quitar la fecha
    textNode.SelectSingleNode(".//div[contains(@class,'date_posted')]")?.Remove();

    // Conservar saltos de línea
    var breaks = textNode.SelectNodes(".//br")?.ToList() ?? [];
    foreach (var br in breaks)
      br.ParentNode.ReplaceChild(textNode.OwnerDocument.CreateTextNode("\n"), br);

    var text = HtmlEntity.DeEntitize(textNode.InnerText);
    return MultiBlankLines().Replace(text.Trim(), "\n\n");
  }

  private static string BuildId(string gameId, string content)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{gameId}|{content}"));
    return Convert.ToHexString(bytes)[..32];
  }

  private static double ParseHours(string? text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return 0;

    var match = Regex.Match(text, @"[\d.,]+");
    if (!match.Success)
      return 0;

    var number = match.Value.Replace(",", "");
    return double.TryParse(number, NumberStyles.Float,
      CultureInfo.InvariantCulture, out var hours)
      ? hours
      : 0;
  }

  [GeneratedRegex(@"\n{3,}")]
  private static partial Regex MultiBlankLines();

  // Clase auxiliar interna para no arrastrar IsGood/IsBad antes de tiempo
  private sealed class RawComment
  {
    public string Id { get; init; } = "";
    public string Content { get; init; } = "";
    public double Hours { get; init; }
    public string GameTitle { get; init; } = "";
  }
}