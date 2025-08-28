using System.Net.Http.Headers;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Zipoid.API;
using Zipoid.API.API.Middlewares;
using Zipoid.API.Application;
using Zipoid.API.Application.Security;
using Zipoid.API.Domain;
using Zipoid.API.Infrastructure.Cache;
using Zipoid.API.Infrastructure.Context;
using Zipoid.API.Infrastructure.Repositories;
using Zipoid.API.Infrastructure.Spotify;
using Zipoid.API.Infrastructure.Spotify.Playlist;
using Zipoid.API.Models;
using static System.Net.Mime.MediaTypeNames;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter JWT token like: Bearer {your_token}",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        }
    );
});

//Dependencies
builder.Services.AddScoped<ISpotifyTokenProvider, SpotifyTokenProvider>();
builder.Services.AddScoped<ISpotifyPlaylistProvider, SpotifyPlaylistProvider>();
builder.Services.AddScoped<ITrackMetadataCacher, TrackMetadataCacher>();
builder.Services.AddScoped<ISearchEngine, SearchService>();
builder.Services.AddScoped<IDownload, DownloadService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJsonWebToken, Zipoid.API.Application.Security.JsonWebToken>();
//TEST
builder.Services.AddScoped<ICoordinator, Coordinator>();


var spotify = new Spotify
{
    Client_ID = Environment.GetEnvironmentVariable("CLIENT_ID") ?? throw new Exception("Empty CLIENT_ID"),
    Client_Secret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? throw new Exception("Empty CLIENT_SECRET")
};

var jwt = new JWT
{
    Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("Empty JWT_KEY"),
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new Exception("Empty JWT_ISSUER"),
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new Exception("Empty JWT_AUDIENCE"),
    DurationInMinutes = double.Parse(Environment.GetEnvironmentVariable("JWT_DurationInMinutes") ?? throw new Exception("Empty JWT_DurationInMinutes"))
};

builder.Services.AddSingleton(spotify);
builder.Services.AddSingleton(jwt);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ISpotifyPlaylistProvider, SpotifyPlaylistProvider>(client =>
{

    client.BaseAddress = new Uri("https://api.spotify.com/v1/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", "BQBbtyBiWLNFLkeAfuWWhbjOmE4KA7zKOYFTS8bJX-zP8rLDXFG_-pOm7vwU3yv-Nvd1-I8kM7O7LmR1Cc5K9FHH4UhdRkABhHsL_pbAWcTXGM9Gmq3lrtlS_osXVP5cJSYuuOW7mGk");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis") ?? throw new Exception("Empty Redis.ConnectionString");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.Zero
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
