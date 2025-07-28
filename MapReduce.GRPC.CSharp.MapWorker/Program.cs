using MapReduce.GRPC.CSharp.MapWorker;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        // gRPC requires HTTP/2
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register gRPC implementation
builder.Services.AddGrpc();
builder.Services.AddSingleton<MapServiceImpl>();

var app = builder.Build();

// Map the gRPC service to ASP.NET Core
app.MapGrpcService<MapServiceImpl>();


// Listen on port 5001
app.Run("http://0.0.0.0:5001");
