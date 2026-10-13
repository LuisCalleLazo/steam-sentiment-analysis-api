using SteamSentimentAnalysis.Context;
using SteamSentimentAnalysis.Repositories;
using SteamSentimentAnalysis.Services;
using SteamSentimentAnalysis.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Services
builder.Services.AddScoped<ISentimentAnalysis, SentimentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHttpClient<IGameService, GameService>();
builder.Services.AddHttpClient<ICommentService, CommentService>();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", policy =>
  {
    policy
      .AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();
app.Run();