using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Load config from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("Gateway is running"));

// Use the YARP middleware
app.MapReverseProxy();

app.Run();
