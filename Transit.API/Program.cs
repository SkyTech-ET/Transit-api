using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using Transit.Api.Filters;
using Transit.API.Services;
using Transit.Application.DataSeeder;
using Transit.Application.Options;
using Transit.Domain.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,                      // publish folder
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")  // static files folder
});
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllUsersQuery).Assembly));


builder.Services.AddControllers(config =>
{
    config.Filters.Add(typeof(ExceptionHandler));
})
.AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddHostedService<LogCleanupService>();

// --- DATABASE CONFIGURATION ---
//string connectionString;

//// Use different connection strings for Production vs Development
//if (builder.Environment.IsProduction())
//{
//    connectionString = builder.Configuration.GetConnectionString("ProductionConnection");
//}
//else
//{
//    connectionString = builder.Configuration.GetConnectionString("DevelopmentConnection");
//}

//// Configure DbContext with SQL Server
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString, sqlOptions =>
//        sqlOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,                // retry up to 5 times
//            maxRetryDelay: TimeSpan.FromSeconds(10), // wait 10s between retries
//            errorNumbersToAdd: null          // apply to all transient errors
//        )
//    )
//);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevelopmentConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileAppPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5000",
                "http://localhost:5002",
                  "https://localhost:5002",
                "http://localhost:8081",
                "http://localhost:5001",
                "https://192.168.43.215:7236",
                "http://192.168.43.215:7236",
                "http://10.0.2.2:7236",
                "http://127.0.0.1:7236"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition", "Content-Length", "Content-Type")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// --- SERVICE INSTALLERS ---
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TokenHandlerService>();
builder.Services.AddScoped<EmailSenderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
//builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IDataSeederService, DataSeederService>();
builder.Services.AddScoped<Transit.API.TestScripts.EndToEndTest>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options => options.IOTimeout = TimeSpan.FromMinutes(10));
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

builder.Services.AddMvc(setupAction: options =>
{
    options.Filters.Add(typeof(AuthorizationHandler));
    options.EnableEndpointRouting = false;
});
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSettings = new JwtSettings();
builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);
var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
builder.Services.Configure<JwtSettings>(jwtSection);
// ----------------- JWT AUTHENTICATION -----------------
var jwtConfig = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audiences[0], // ? Correct
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)) // ? Correct
        };
    });


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listenOptions => listenOptions.UseHttps());

});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    AdminPrivilegeSeeder.Seed(scope.ServiceProvider);
}

// Static files for profile photos
var profilePhotoPath = Path.Combine(builder.Environment.WebRootPath, "Profile_Photo");

// Ensure folder exists (optional)
if (!Directory.Exists(profilePhotoPath))
{
    Directory.CreateDirectory(profilePhotoPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profilePhotoPath),
    RequestPath = "/api/v1/Profile_Photo"
});

app.UseAuthentication();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseCors("MobileAppPolicy");
app.UseStaticFiles();
app.UseSession();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
