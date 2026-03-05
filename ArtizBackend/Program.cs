using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using DAL;
using DAL.Repositories;
using BLL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Allow large file uploads (e.g. big .glb models)
builder.Services.Configure<FormOptions>(options =>
{
    const long maxBodySize = 200_000_000; // 200 MB
    options.MultipartBodyLengthLimit = maxBodySize;
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Also increase Kestrel max request body size
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200_000_000;
});

// Configure Entity Framework - Supabase (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("Supabase");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IEmailEventPublisher, ArtizBackend.Services.KafkaEmailEventPublisher>();

// Background email queue (non-blocking email sending)
builder.Services.AddSingleton<IEmailQueue, ArtizBackend.Services.InMemoryEmailQueue>();
builder.Services.AddHostedService<ArtizBackend.Services.EmailBackgroundService>();

// Storage: use Cloudflare R2 when configured, else LocalStorage for development
var r2AccountId = builder.Configuration["CloudflareR2:AccountId"];
if (!string.IsNullOrWhiteSpace(r2AccountId) && r2AccountId != "YOUR_ACCOUNT_ID")
    builder.Services.AddScoped<IStorageService, CloudflareR2StorageService>();
else
    builder.Services.AddScoped<IStorageService, LocalStorageService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Artiz API",
        Version = "v1",
        Description = "API for Artiz E-commerce Platform"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Artiz API V1");
    });
}

// CORS must be before UseHttpsRedirection to avoid redirect issues in preflight requests
app.UseCors("AllowFrontend");

// Only use HTTPS redirection if not in development or if explicitly needed
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Serve uploaded files (when using LocalStorage)
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

app.MapControllers();

// Apply migrations (Code First) - do not crash app if DB is unreachable
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "An error occurred while migrating or initializing the database. " +
            "The application will continue to run, but database-dependent features may not work correctly.");
    }
}

app.Run();

