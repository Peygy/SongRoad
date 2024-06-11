using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MainApp.Models.User;
using MainApp.Services;
using System.Net;
using MainApp.Data;
using MainApp.Interfaces.Music;
using MainApp.Interfaces.Entry;
using MainApp.Interfaces.User;
using MainApp.Interfaces.Crew;
using MainApp.Services.Crew;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Connecting a Data Context for an Application Release
#if RELEASE
string? connectionUser = configuration.GetConnectionString("UserDb");
// Connecting a Data Context for Application Testing
#elif DEBUG
string? connectionUser = configuration["ConnectionStrings:TestUserDb"];
#endif

// Adding DataContexts to the application
builder.Services.AddDbContext<UserContext>(options => options.UseNpgsql(connectionUser));

// Dependency injection for services
// For auth services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtDataService, JwtDataService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddSingleton<IJwtGenService, JwtGenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// For crew services
builder.Services.AddScoped<IUserManageService, UserManageService>();
builder.Services.AddScoped<ICrewManageService, CrewManageService>();
// For user services
builder.Services.AddScoped<IUserService, UserService>();
// For music services
builder.Services.AddSingleton<GoogleDriveApiService>();
builder.Services.Configure<MusicContext>(configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoService>();
builder.Services.AddScoped<IMusicService, MusicService>();

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

// Node_modules folder support
var env = app.Services.GetService<IHostEnvironment>();
if (env != null)
{
    app.UseFileServer(new FileServerOptions()
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(env.ContentRootPath, "node_modules")),
        RequestPath = "/node_modules",
        EnableDirectoryBrowsing = false
    });
}

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
    pattern: "{controller=Crew}/{action}",
    defaults: new { controller = "Crew" });

app.MapControllerRoute(
    name: "user",
    pattern: "{action}",
    defaults: new { controller = "User" });

app.Run();