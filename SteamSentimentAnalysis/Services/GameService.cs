using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SteamSentimentAnalysis.DTOs;
using SteamSentimentAnalysis.Services.Interfaces;

namespace SteamSentimentAnalysis.Services;

public class GameService(HttpClient httpClient) : IGameService
{
  public async Task<List<DropdownOptionDto>> GetGamesOfSteam(
    string search)
  {
    if (string.IsNullOrWhiteSpace(search))
      return [];

    var url =
      $"https://store.steampowered.com/search/?term={Uri.EscapeDataString(search)}";

    using var request = new HttpRequestMessage(HttpMethod.Get, url);

    request.Headers.UserAgent.ParseAdd(
      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
      "AppleWebKit/537.36 (KHTML, like Gecko) " +
      "Chrome/130.0.0.0 Safari/537.36");

    request.Headers.AcceptLanguage.ParseAdd("es-ES,es;q=0.9");

    using var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var html = await response.Content.ReadAsStringAsync();

    var document = new HtmlDocument();
    document.LoadHtml(html);

    var rows = document.DocumentNode.SelectNodes(
      "//*[@id='search_resultsRows']/a[contains(@class, 'search_result_row')]");

    return rows
      .Select(row =>
      {
        var id = row.GetAttributeValue("data-ds-appid", "");

        var name = row.SelectSingleNode(
            ".//span[contains(@class, 'title')]")
          ?.InnerText.Trim();

        return new DropdownOptionDto
        {
          Id = id,
          Name = HtmlEntity.DeEntitize(name ?? "")
        };
      })
      .Where(game =>
        !string.IsNullOrWhiteSpace(game.Id) &&
        !string.IsNullOrWhiteSpace(game.Name))
      .Take(20)
      .ToList();
  }

  public async Task<GameDto> GetGameDetail(string gameId)
  {
    var url = $"https://store.steampowered.com/app/{Uri.EscapeDataString(gameId)}/?l=english";

    using var request = new HttpRequestMessage(HttpMethod.Get, url);

    request.Headers.UserAgent.ParseAdd(
      "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
      "AppleWebKit/537.36 (KHTML, like Gecko) " +
      "Chrome/130.0.0.0 Safari/537.36");

    request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

    // Evita la pantalla de verificación de edad
    request.Headers.Add(
      "Cookie",
      "birthtime=568022401; lastagecheckage=1-0-1988; mature_content=1");

    using var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var html = await response.Content.ReadAsStringAsync();

    var document = new HtmlDocument();
    document.LoadHtml(html);

    var root = document.DocumentNode;

    var title = Clean(root.SelectSingleNode(
      "//*[@id='appHubAppName']")?.InnerText);

    if (string.IsNullOrWhiteSpace(title))
      throw new KeyNotFoundException(
        $"No se encontró el juego con id '{gameId}'.");

    var description = Clean(root.SelectSingleNode(
      "//div[contains(@class, 'game_description_snippet')]")?.InnerText);

    var developer = Clean(root.SelectSingleNode(
      "//*[@id='developers_list']//a")?.InnerText);

    var dateText = Clean(root.SelectSingleNode(
      "//div[contains(@class, 'release_date')]//div[contains(@class, 'date')]")
      ?.InnerText);

    var category = Clean(root.SelectSingleNode(
      "(//div[contains(@class, 'blockbg')]//a)[2]")?.InnerText);

    var imageUrl = root.SelectSingleNode(
        "//div[contains(@class,'game_background_glow')]//*[@id='gameHeaderImageCtn']//img")
      ?.GetAttributeValue("src", "") ?? "";

    return new GameDto
    {
      Id = gameId,
      Title = title,
      Description = description,
      Date = ParseDate(dateText),
      Developer = developer,
      Category = category,
      ImageUrl = imageUrl
    };
  }

  private static string Clean(string? text) =>
    HtmlEntity.DeEntitize(text ?? "").Trim();

  private static DateTime ParseDate(string text)
  {
    string[] formats =
    [
      "d MMM, yyyy", "dd MMM, yyyy", "MMM d, yyyy", "MMM yyyy", "yyyy"
    ];

    foreach (var culture in new[] { "en-US", "es-ES" })
    {
      if (DateTime.TryParseExact(
            text,
            formats,
            CultureInfo.GetCultureInfo(culture),
            DateTimeStyles.None,
            out var date))
        return date;
    }

    return default;
  }
}