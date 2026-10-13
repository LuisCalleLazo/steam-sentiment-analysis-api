using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SteamSentimentAnalysis.Entities;

public class Comment
{
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } = null!;
  public string Content { get; set; } = null!;
  public DateTime Date { get; set; }
  public double HoursGameplay { get; set; }
  public bool IsBad { get; set; }
  public bool IsGood { get; set; }
  public string GameId { get; set; } = null!;
  public string GameTitle { get; set; } = null!;
}