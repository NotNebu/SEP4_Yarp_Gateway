using ApiGateway.Application.Interfaces;
using ApiGateway.Infrastructure.GrpcClients;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserService.Grpc;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Konfigurer Kestrel til at understøtte både HTTP/1.1 og HTTP/2 (krævet for gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(
        5107, // Gateway lytter på port 5107
        listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        }
    );
});

// Tillad alle CORS-anmodninger
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Registrer IUserService-implementationen, som benytter gRPC til at tale med user-service
builder.Services.AddScoped<IUserService, UserServiceClient>();

// Tilføj støtte for MVC/Web API controllere
builder.Services.AddControllers();

// Konfigurer YARP Reverse Proxy vha. appsettings.json (sektionen "ReverseProxy")
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Konfigurer gRPC-klienten til AuthService, og peger mod user-service-containeren via Docker-netværk
builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri("http://user-service:5001"); // internt DNS-navn i docker-compose
});

var app = builder.Build();

// Brug CORS-politikken defineret ovenfor
app.UseCors("AllowAll");

// Simpelt healthcheck-endpoint
app.MapGet("/health", () => Results.Ok("Gateway is running"));

// Map reverse proxy-r
app.MapReverseProxy();

// Mapper REST-kontrollere
app.MapControllers();

app.Run();