using SteamSentimentAnalysis.DTOs;

namespace SteamSentimentAnalysis.Services.Interfaces;

public interface IGameService
{
  Task<List<DropdownOptionDto>> GetGamesOfSteam(string search);
  Task<GameDto> GetGameDetail(string gameId);
}