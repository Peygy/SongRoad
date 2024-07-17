using MainApp.Services.Music;
using MainApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = builder.Configuration["RedisConnection"];
});

builder.Services.AddSingleton<ITracksCachingService, TracksCachingService>();
builder.Services.AddSingleton<IGoogleDriveApi, GoogleDriveApi>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
