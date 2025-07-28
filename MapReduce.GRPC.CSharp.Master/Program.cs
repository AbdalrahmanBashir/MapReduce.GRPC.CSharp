using Grpc.Core;
using Mapreduce;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.ListenAnyIP(5075, o => o.Protocols =
        Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

// Add services to the container.

builder.Services.AddGrpcClient<MapService.MapServiceClient>(
    o =>
    {
        o.Address = new Uri("http://mapworker:5001");
    });

builder.Services.AddGrpcClient<ShuffleService.ShuffleServiceClient>(
    o =>
    {
        o.Address = new Uri("http://shuffleworker:5002");

    });

builder.Services.AddGrpcClient<ReduceService.ReduceServiceClient>(
    o =>
    {
        o.Address = o.Address = new Uri("http://reduceworker:5003");
    });

var app = builder.Build();

await RunPipelineAsync(app.Services, args);

return;


async Task RunPipelineAsync(IServiceProvider services, string[] args)
{
    var mapClient = services.GetRequiredService<MapService.MapServiceClient>();
    var shuffleClient = services.GetRequiredService<ShuffleService.ShuffleServiceClient>();
    var reduceClient = services.GetRequiredService<ReduceService.ReduceServiceClient>();

    // Get the input DOCX file path from command line args or use a default one
    var docxPath = args.Length > 0
        ? args[0]
        : Path.Combine(AppContext.BaseDirectory, "alice-in-wonderland.docx");

    if (!File.Exists(docxPath))
        throw new FileNotFoundException($"Input DOCX not found: {docxPath}");

    // Read the DOCX file and extract lines of text
    List<string> linesToMap = new List<string>();
    using var fs = new FileStream(docxPath, FileMode.Open,FileAccess.Read,FileShare.ReadWrite );

    // Open the WordprocessingDocument to read its content
    using var wordDoc = WordprocessingDocument.Open(fs, false);
    var body = wordDoc.MainDocumentPart?.Document?.Body;
    if (body != null)
    {
        foreach (var para in body.Elements<Paragraph>())
        {
            var text = para.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
                linesToMap.Add(text);
        }
    }


    // Map each line to a (word, count) pair
    //    using the MapService gRPC client
    Console.WriteLine("Starting MapReduce pipeline...");
    Console.WriteLine("Mapping lines to (word, count) pairs...");

    using var mapCall = mapClient.Map();
    foreach (var line in linesToMap)
        await mapCall.RequestStream.WriteAsync(new Line { Text = line });
    await mapCall.RequestStream.CompleteAsync();

    var intermediate = new List<WordCount>();
    await foreach (var wc in mapCall.ResponseStream.ReadAllAsync())
        intermediate.Add(wc);

    // Shuffle by bucket
    var buckets = new Dictionary<int, List<WordCount>>();
    int R = 3;
    foreach (var wc in intermediate)
    {
        int bucket = Math.Abs(wc.Word.GetHashCode()) % R;
        if (!buckets.TryGetValue(bucket, out var list))
            buckets[bucket] = list = new();
        list.Add(wc);
    }

    // Send each bucket to its shuffle worker
    var reducedIntermediate = new Dictionary<string, int>();
    foreach (var kv in buckets)
    {
        using var shuffleCall = shuffleClient.Shuffle();
        foreach (var wc in kv.Value)
            await shuffleCall.RequestStream.WriteAsync(wc);
        await shuffleCall.RequestStream.CompleteAsync();

        await foreach (var wc in shuffleCall.ResponseStream.ReadAllAsync())
            reducedIntermediate[wc.Word] = reducedIntermediate.GetValueOrDefault(wc.Word) + wc.Count;
    }

    // Now send the reduced intermediate to the reduce worker
    using var reduceCall = reduceClient.Reduce();
    foreach (var kv in reducedIntermediate)
        await reduceCall.RequestStream.WriteAsync(new WordCount { Word = kv.Key, Count = kv.Value });
    await reduceCall.RequestStream.CompleteAsync();

    var finalResults = new Dictionary<string, int>();
    await foreach (var wc in reduceCall.ResponseStream.ReadAllAsync())
    {
        finalResults[wc.Word] = wc.Count;
        Console.WriteLine($"Reduced: {wc.Word} → {wc.Count}");
    }

    Console.WriteLine("MapReduce pipeline completed successfully.");


    Environment.Exit(0);
};
