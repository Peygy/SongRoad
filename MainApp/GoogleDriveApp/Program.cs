using GoogleDriveApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = builder.Configuration["RedisConnection"];
});

builder.Services.AddGrpc();

builder.Services.AddSingleton<ITracksCachingService, TracksCachingService>();
builder.Services.AddSingleton<IGoogleDriveApi, GoogleDriveApi>();

var app = builder.Build();

app.MapGrpcService<FileDownloaderService>();
app.MapGet("/", () => "Hello World!");

app.Run();
