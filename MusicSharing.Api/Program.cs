using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Services;
using MusicSharing.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MusicSharing API",
        Version = "v1"
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

builder.WebHost.ConfigureKestrel(options =>
{
    var certPath = builder.Configuration["CERT_PATH"];
    var certPassword = builder.Configuration["CERT_PASSWORD"];
    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
    {
        options.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.UseHttps(certPath, certPassword);
        });
        options.ListenAnyIP(5000); // HTTP
    }
});

// Services
builder.Services.AddScoped<IMusicService, MusicService>();
builder.Services.AddScoped<MusicService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<RatingService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<FollowerService>();
builder.Services.AddScoped<ActivityService>();


var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(c =>
    {
        c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicSharing API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
