using ApiGateway.Application.Interfaces;
using ApiGateway.Infrastructure.GrpcClients;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserService.Grpc;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Konfigurer Kestrel til både HTTP/1.1 og HTTP/2 (krævet for gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(
        5107,
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        }
    );
});

// CORS-politik for frontend (dev + Docker)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "http://growheat-frontend:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Registrer gRPC-klient og services
builder.Services.AddScoped<IUserService, UserServiceClient>();
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri("http://user-service:5001");
});

// Tilføj controllere
builder.Services.AddControllers();

// Konfigurer YARP + tilføj CORS headers til proxy-responses
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

// Routing og CORS først
app.UseRouting();
app.UseCors("AllowFrontend");

// Håndter preflight OPTIONS requests
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

// Healthcheck endpoint
app.MapGet("/health", () => Results.Ok("Gateway is running"));

// Map controllere og proxy
app.MapControllers();
app.MapReverseProxy();

app.Run();
