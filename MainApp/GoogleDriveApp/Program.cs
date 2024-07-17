using GoogleDriveApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = builder.Configuration["RedisConnection"];
});

builder.Services.AddGrpc(option =>
{
    option.MaxSendMessageSize = 16 * 1024 * 1024;
    option.MaxReceiveMessageSize = 16 * 1024 * 1024;
});

builder.Services.AddSingleton<ITracksCachingService, TracksCachingService>();
builder.Services.AddSingleton<IGoogleDriveApi, GoogleDriveApi>();

var app = builder.Build();

app.MapGrpcService<GoogleDriveAppConnectorService>();
app.MapGet("/", () => "Hello World!");

app.Run();