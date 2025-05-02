using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using UserService.Grpc;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ------------------- KESTREL -------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5107, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

// ------------------- CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5174",
                "http://localhost:3000",
                "http://growheat-frontend:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ------------------- JWT via COOKIE -------------------
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
                // Hent fra Cookie
                var cookieToken = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    Console.WriteLine("Token hentet fra cookie.");
                    context.Token = cookieToken;
                    return Task.CompletedTask;
                }

                // TIL POSTMAN TESTING (DEN FORSTÅR IKKE COOKIES LOL (._.)
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length);
                    Console.WriteLine("Token hentet fra Authorization header.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// ------------------- gRPC-klienter -------------------
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri("http://user-service:5001");
});

builder.Services.AddHttpClient("MalAPI", c =>
{
    c.BaseAddress = new Uri("http://Sep4-API-Service:8080");
});

builder.Services.AddHttpClient("IotAPI", c =>
{
    c.BaseAddress = new Uri("http://iot-container:8080");
});

// ------------------- Controllers & YARP -------------------
builder.Services.AddControllers();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddResponseHeader("Access-Control-Allow-Origin", "http://localhost:3000", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Credentials", "true", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS", append: false);
        builderContext.AddResponseHeader("Access-Control-Allow-Headers", "*", append: false);
    });

var app = builder.Build();

// ------------------- Middleware Pipeline -------------------

app.UseRouting();

app.UseCors("AllowFrontend");

// Preflight OPTIONS-handler
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

// Auth Middleware
app.UseAuthentication();
app.UseAuthorization();

// Healthcheck
app.MapGet("/health", () => Results.Ok("Gateway is running"));

// API Routes
app.MapControllers();
app.MapReverseProxy();

app.Run();
