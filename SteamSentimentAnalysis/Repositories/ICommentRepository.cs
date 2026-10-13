using SteamSentimentAnalysis.Entities;

namespace SteamSentimentAnalysis.Repositories;

public interface ICommentRepository
{
  Task<List<Comment>> GetAllAsync();

  Task<Comment?> GetByIdAsync(string id);

  Task<List<Comment>> GetByGameIdAsync(string gameId);

  Task CreateAsync(Comment comment);

  Task CreateManyAsync(List<Comment> comments);

  Task UpdateAsync(string id, Comment comment);

  Task DeleteAsync(string id);
}