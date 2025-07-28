
using MapReduce.GRPC.CSharp.ReduceWorker;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003, listenOptions =>
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
builder.Services.AddSingleton<ReduceServiceImpl>();

var app = builder.Build();

// Map the gRPC service to ASP.NET Core
app.MapGrpcService<ReduceServiceImpl>();


// Listen on port 5003
app.Run("http://0.0.0.0:5003");

