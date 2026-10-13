using MongoDB.Driver;
using SteamSentimentAnalysis.Context;
using SteamSentimentAnalysis.Entities;

namespace SteamSentimentAnalysis.Repositories;

public class CommentRepository(MongoDbContext context) : ICommentRepository
{
  private readonly IMongoCollection<Comment> _comments = context.Comments;

  public async Task<List<Comment>> GetAllAsync()
  {
    return await _comments
      .Find(FilterDefinition<Comment>.Empty)
      .ToListAsync();
  }

  public async Task<Comment?> GetByIdAsync(string id)
  {
    return await _comments
      .Find(c => c.Id == id)
      .FirstOrDefaultAsync();
  }

  public async Task<List<Comment>> GetByGameIdAsync(string gameId)
  {
    return await _comments
      .Find(c => c.GameId == gameId)
      .ToListAsync();
  }

  public async Task CreateAsync(Comment comment)
  {
    await _comments.InsertOneAsync(comment);
  }

  public async Task CreateManyAsync(List<Comment> comments)
  {
    var list = comments.ToList();

    if (list.Count == 0)
      return;

    await _comments.InsertManyAsync(list);
  }

  public async Task UpdateAsync(string id, Comment comment)
  {
    comment.Id = id;

    await _comments.ReplaceOneAsync(
      c => c.Id == id,
      comment);
  }

  public async Task DeleteAsync(string id)
  {
    await _comments.DeleteOneAsync(c => c.Id == id);
  }
}