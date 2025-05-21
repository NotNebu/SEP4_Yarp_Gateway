using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ------------------- KESTREL -------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5107, listenOptions =>
    {
        listenOptions.UseHttps("/https/localhost-user-service.p12", "changeit");
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

// ------------------- KONFIGURATIONER (YARP-SPLIT) -------------------
builder.Configuration
    .AddJsonFile("Configuration/Routes.Auth.json", optional: true)
    .AddJsonFile("Configuration/Routes.User.json", optional: true)
    .AddJsonFile("Configuration/Routes.Iot.json", optional: true)
    .AddJsonFile("Configuration/Routes.Mal.json", optional: true)
    .AddJsonFile("Configuration/Clusters.json", optional: true)
    .AddEnvironmentVariables();

// ------------------- CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ------------------- JWT VIA COOKIE -------------------
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET er ikke sat i .env");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookieToken = context.Request.Cookies["jwt"];
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (!string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                }
                else if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length);
                }

                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// ------------------- CONTROLLERS + YARP -------------------
builder.Services.AddControllers();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddResponseHeader("Access-Control-Allow-Origin", "https://localhost:3000", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Credentials", "true", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Headers", "*", append: false);
    });

var app = builder.Build();

// ------------------- MIDDLEWARE PIPELINE -------------------
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
});

app.UseRouting();
app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

// ------------------- ROUTES -------------------
app.MapControllers();
app.MapReverseProxy();

app.Run();