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

var builder = WebApplication.CreateBuilder(args);

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
string connectionString;

// Use Postgres in production, SQLite for local dev
if (builder.Environment.IsProduction())
{
    connectionString = builder.Configuration.GetConnectionString("ProductionConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
}
else
{
    // SQLite for development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString)
    );


}

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileAppPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5000",
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
builder.Services.AddScoped<IDocumentService, DocumentService>();
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
            ValidAudience = jwtSettings.Audiences[0], // ✅ Correct
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)) // ✅ Correct
        };
    });
var app = builder.Build();

// Static files for profile photos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "Profile_Photo")),
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
