using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MainApp.Models.User;
using System.Net;
using MainApp.Data;
using MainApp.Services.Crew;
using MainApp.Services.Music;
using MainApp.Services.Entry;
using MainApp.Middleware;
using MainApp.Protos;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Connecting a Data Context for an Application Release
#if RELEASE
string? connectionUser = configuration.GetConnectionString("UserDb");
// Connecting a Data Context for Application Testing
#elif DEBUG
var connectionUser = configuration.GetConnectionString("TestUserDb");
#endif

// Adding UserContext to the application
builder.Services.AddDbContext<UserContext>(options => options.UseNpgsql(connectionUser));
// Adding MusicContext to the application
var mongoDBSettings = configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
builder.Services.Configure<MongoDBSettings>(configuration.GetSection("MongoDBSettings"));
builder.Services.AddDbContext<MusicContext>(options => 
    options.UseMongoDB(mongoDBSettings.ConnectionURL, mongoDBSettings.DatabaseName));

// Dependency injection for services
// For auth services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRefershTokenService, RefershTokenService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddSingleton<IJwtGenService, JwtGenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// For crew services
builder.Services.AddScoped<IUserManageService, UserManageService>();
builder.Services.AddScoped<ICrewManageService, CrewManageService>();
// For music services
builder.Services.AddScoped<IMongoService, MongoService>();
builder.Services.AddScoped<IMusicService, MusicService>();
// For microservices connectors
builder.Services.AddSingleton<IGoogleDriveAppConnectorService, GoogleDriveAppConnectorService>();

// Add Indentity in project
builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
{
    options.Password.RequiredLength = 7;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
}).AddEntityFrameworkStores<UserContext>();

// Adding JWT tokens to services
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // For change 302 to 401
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidIssuer = configuration["JwtSettings:ISSUER"],
        ValidAudience = configuration["JwtSettings:AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:KEY"]!)),
    };
});

// Converting all queries to lowercase for easy of use, for example: ~/Main/View changes to ~/main/view
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// Controllers settings
#if RELEASE
builder.Services.AddControllersWithViews();
#elif DEBUG
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif

// Configuring Grpc clients to add connection configurations to Grpc servers in DI
builder.Services.AddGrpcClient<GoogleDriveConnector.GoogleDriveConnectorClient>(options =>
{
    options.Address = new Uri(configuration.GetSection("GoogleDriveAppAddress").Value);
})
.ConfigureChannel(options =>
{
    options.MaxReceiveMessageSize = 16 * 1024 * 1024;
    options.MaxSendMessageSize = 16 * 1024 * 1024;
});

var app = builder.Build();

// Initializing of identity database entities
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var userManager = services.GetRequiredService<UserManager<UserModel>>();
        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var config = services.GetRequiredService<IConfiguration>();
        await IdentityInitializer.InitializeAsync(userManager, rolesManager, config);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при добавлении в базу данных сущностей");
    }
}

// Error handler settings
#if RELEASE
// For view server exceptions  	
app.UseStatusCodePagesWithReExecute("/error", "?code={0}");
#elif DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseStatusCodePages(async context =>
{
    var statusCode = context.HttpContext.Response.StatusCode;
    if (statusCode == (int)HttpStatusCode.Unauthorized || statusCode == (int)HttpStatusCode.Forbidden)
    {
        context.HttpContext.Response.Cookies.Delete("access_token");
        context.HttpContext.Response.Cookies.Delete("refresh_token");

        context.HttpContext.Response.Redirect("/login");
    }
});

#if RELEASE
// Add SSL for HTTPS 
app.UseHsts();
#endif

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

// Middleware for check/update JWT tokens
app.UseMiddleware<CheckTokenMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{action}",
    defaults: new { controller = "Page", action = "Welcome" });

app.MapControllerRoute(
    name: "auth",
    pattern: "{action}",
    defaults: new { controller = "Auth" });

app.MapControllerRoute(
    name: "crew",
    pattern: "{controller}/{action}",
    defaults: new { controller = "Crew" });

app.MapControllerRoute(
    name: "user",
    pattern: "{action}",
    defaults: new { controller = "User" });

app.Run();

public partial class Program { }