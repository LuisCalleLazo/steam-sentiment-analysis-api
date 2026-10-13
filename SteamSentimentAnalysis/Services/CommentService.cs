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
  ICommentRepository commentRepository) : ICommentService
{
  private const int MaxPages = 5;

  public async Task<List<CommentDto>> GetCommentsByGameId(string gameId)
  {
    var scraped = new Dictionary<string, Comment>();

    for (var page = 1; page <= MaxPages; page++)
    {
      var comments = await ScrapePage(gameId, page);
      if (comments.Count == 0)
        break;

      // Si la página no aportó nada nuevo, Steam está repitiendo contenido
      var added = comments.Count(c => scraped.TryAdd(c.Id, c));
      if (added == 0)
        break;

      await Task.Delay(500); // no saturar a Steam
    }

    if (scraped.Count == 0)
      return [];

    // Evitar duplicados en la base de datos
    var existing = await commentRepository.GetByGameIdAsync(gameId);
    var existingIds = existing.Select(c => c.Id).ToHashSet();

    var toInsert = scraped.Values
      .Where(c => !existingIds.Contains(c.Id))
      .ToList();

    if (toInsert.Count > 0)
      await commentRepository.CreateManyAsync(toInsert);

    return scraped.Values
      .Select(c => new CommentDto
      {
        Comment = c.Content,
        Date = c.Date,
        HoursGameplay = c.HoursGameplay
      })
      .ToList();
  }

  private async Task<List<Comment>> ScrapePage(string gameId, int page)
  {
    var url =
      $"https://steamcommunity.com/app/{Uri.EscapeDataString(gameId)}/reviews/" +
      $"?p={page}&browsefilter=mostrecent&filterLanguage=spanish&l=spanish";

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

    var gameTitle = HtmlEntity.DeEntitize(
      document.DocumentNode.SelectSingleNode(
        "//div[contains(@class,'apphub_AppName')]")?.InnerText ?? "").Trim();

    return (from card in cards
      let content = ExtractContent(card)
      where !string.IsNullOrWhiteSpace(content)
      let dateText = card.SelectSingleNode(".//div[contains(@class,'date_posted')]")?.InnerText
      let hoursText = card.SelectSingleNode(".//div[contains(@class,'hours')]")?.InnerText
      select new Comment
      {
        Id = BuildId(gameId, content),
        Content = content,
        Date = ParseDate(dateText),
        HoursGameplay = ParseHours(hoursText),
        IsBad = false,
        IsGood = false,
        GameId = gameId,
        GameTitle = gameTitle
      }).ToList();
  }

  private static string ExtractContent(HtmlNode card)
  {
    var textNode = card.SelectSingleNode(
      ".//div[contains(@class,'apphub_CardTextContent')]");

    // Quitar la fecha para que no se mezcle con el texto
    textNode.SelectSingleNode(".//div[contains(@class,'date_posted')]")?.Remove();

    // Conservar saltos de línea
    var breaks = textNode.SelectNodes(".//br")?.ToList() ?? [];
    foreach (var br in breaks)
      br.ParentNode.ReplaceChild(textNode.OwnerDocument.CreateTextNode("\n"), br);

    var text = HtmlEntity.DeEntitize(textNode.InnerText);
    return MultiBlankLines().Replace(text.Trim(), "\n\n");
  }

  // Id determinista: mismo juego + mismo texto = mismo id => sin duplicados
  private static string BuildId(string gameId, string content)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{gameId}|{content}"));
    return Convert.ToHexString(bytes)[..32];
  }

  // "11.6 h registradas" => 11.6
  private static double ParseHours(string? text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return 0;

    var match = Regex.Match(text, @"[\d.,]+");
    if (!match.Success)
      return 0;

    var number = match.Value.Replace(",", ""); // separador de miles
    return double.TryParse(number, NumberStyles.Float,
      CultureInfo.InvariantCulture, out var hours)
      ? hours
      : 0;
  }

  // "Publicada: 12 de octubre" (sin año si es del año actual)
  // "Publicada: 12 de octubre de 2025"
  private static DateTime ParseDate(string? text)
  {
    if (string.IsNullOrWhiteSpace(text))
      return default;

    var clean = Regex.Replace(HtmlEntity.DeEntitize(text).Trim(), @"^[^:]*:\s*", "");

    string[] formats = ["d 'de' MMMM 'de' yyyy", "d 'de' MMMM"];

    if (!DateTime.TryParseExact(clean, formats,
          CultureInfo.GetCultureInfo("es-ES"), DateTimeStyles.None, out var date))
      return default;

    // Sin año, .NET asume el actual; si queda en el futuro era del año pasado
    return date > DateTime.Today ? date.AddYears(-1) : date;
  }

  [GeneratedRegex(@"\n{3,}")]
  private static partial Regex MultiBlankLines();
}