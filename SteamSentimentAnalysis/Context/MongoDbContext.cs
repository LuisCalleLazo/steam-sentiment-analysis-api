using MongoDB.Driver;
using SteamSentimentAnalysis.Entities;
using SteamSentimentAnalysis.Helpers;

namespace SteamSentimentAnalysis.Context;

public class MongoDbContext
{
  public MongoDbContext(IConfiguration configuration)
  {
    var settings = configuration
                     .GetSection("MongoDbSettings")
                     .Get<MongoDbSettings>()
                   ?? throw new InvalidOperationException(
                     "No se encontró la configuración de MongoDB.");

    var client = new MongoClient(settings.ConnectionString);

    var database = client.GetDatabase(settings.DatabaseName);

    Comments = database.GetCollection<Comment>(
      settings.CommentsCollectionName);
  }

  public IMongoCollection<Comment> Comments { get; }
}
