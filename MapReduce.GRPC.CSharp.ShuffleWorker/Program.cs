using MapReduce.GRPC.CSharp.ShuffleWorker;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listenOptions =>
    {
        // gRPC requires HTTP/2
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register gRPC and your implementation
builder.Services.AddGrpc();
builder.Services.AddSingleton<ShuffleServiceImpl>();

var app = builder.Build();

// Map the gRPC service to ASP.NET Core
app.MapGrpcService<ShuffleServiceImpl>();


// Listen on port 5002
app.Run("http://0.0.0.0:5002");
