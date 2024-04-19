using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MainApp.Models;
using MainApp.Services;
using MainApp.Models.Service;
using MainApp.Services.Jwt;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// Connecting a Data Context for an Application Release
#if RELEASE
    string connectionUser = configuration.GetConnectionString("DefaultUserDb");
    string connectionTopics = configuration.GetConnectionString("DefaultTopicDb");
    builder.Services.Configure<ContentDataSettings>(builder.Configuration.GetSection("DefaultContentStore"));
// Connecting a Data Context for Application Testing
#elif DEBUG
    string? connectionUser = configuration.GetConnectionString("TestUserDb");
    //string? connectionTopics = builder.Configuration.GetConnectionString("TestTopicDb");
    //builder.Services.Configure<MongoDataSettings>(builder.Configuration.GetSection("TestContentStore"));
#endif



// Adding DataContexts to the application
builder.Services.AddDbContext<UserContext>(options => options.UseNpgsql(connectionUser));
/*
builder.Services.AddDbContext<PartsContext>(options => options.UseNpgsql(connectionTopics));
// Adding services to the application
//builder.Services.AddScoped<IAdminService, AdminService>();
//builder.Services.AddScoped<IMongoService, MongoService>();
*/

// Dependency injection for services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtDataService, JwtDataService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IJwtGenService, JwtGenService>();
builder.Services.AddScoped<IJwtCheckService, IJwtCheckService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Add Indentity in project
builder.Services.AddIdentity<UserModel, IdentityRole>().AddEntityFrameworkStores<UserContext>();

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
        // Указывает, будет ли валидироваться издатель при валидации токена
        ValidateIssuer = true,
        // Будет ли валидироваться потребитель токена
        ValidateAudience = true,
        // Будет ли валидироваться время существования
        ValidateLifetime = true,
        // Валидация ключа безопасности
        ValidateIssuerSigningKey = true,
        // Чтобы время истекало точно в срок
        ClockSkew = TimeSpan.Zero,

        // Строка, представляющая издателя
        ValidIssuer = configuration["JwtSettings:ISSUER"],
        // Установка потребителя токена
        ValidAudience = configuration["JwtSettings:AUDIENCE"],
        // Установка ключа безопасности
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

// Enviroment settings
#if RELEASE
    // For view server exceptions  	
    app.UseStatusCodePagesWithReExecute("/error", "?code={0}");
#elif DEBUG
    app.UseDeveloperExceptionPage();
#endif

#if RELEASE
    // Add SSL for HTTPS 
    app.UseHsts();
#endif

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

// Middleware for check JWT tokens
app.UseMiddleware<CheckTokenMiddleware>();

// JWT Authentication and Authorization connection for entry
app.Use(async (context, next) =>
{
    var accessToken = context.Request.Cookies["access_token"];
    context.Request.Headers.Add("Authorization", "Bearer " + accessToken);
    await next().ConfigureAwait(false);
});

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
    pattern: "{action}",
    defaults: new { controller = "Crew" });

app.Run();