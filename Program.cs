using Yarp.ReverseProxy;
using UserServiceProto;

var builder = WebApplication.CreateBuilder(args);

// Tvinger http for at teste Curl :~D
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5107); // HTTP
});

// Forbindelse til gRPC User Klienten
builder.Services.AddGrpcClient<Greeter.GreeterClient>(o =>
{
    o.Address = new Uri("http://localhost:5001"); 
});

builder.Services.AddHttpClient("IotAPI", c =>
{
    var baseUrl = builder.Configuration["SpringBootApi:BaseUrl"] ?? "http://localhost:8080";
    c.BaseAddress = new Uri(baseUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddControllers();

// Load config from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("Gateway is running"));

// Use the YARP middleware
app.MapReverseProxy();

app.Run();
