using Grpc.Core;
using Mapreduce;

namespace MapReduce.GRPC.CSharp.MapWorker
{

    public class MapServiceImpl : MapService.MapServiceBase
    {
        private readonly ILogger<MapServiceImpl> _logger;
        public MapServiceImpl(ILogger<MapServiceImpl> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Map operation that processes a stream of lines and emits (word, count) pairs.
        // It reads lines from the request stream, splits them into words, and counts occurrences.

        public override async Task Map(IAsyncStreamReader<Line> requestStream, IServerStreamWriter<WordCount> responseStream, ServerCallContext context)
        {
            var counts = new Dictionary<string, int>();

            _logger.LogInformation("Starting Map operation...");
            // Read the stream of lines
            await foreach (var line in requestStream.ReadAllAsync())
            {
                foreach (var w in line.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    counts[w] = counts.GetValueOrDefault(w) + 1;
            }

            // Write back all (word,count) pairs
            foreach (var kv in counts)
            {
                await responseStream.WriteAsync(new WordCount { Word = kv.Key, Count = kv.Value });
            }
            _logger.LogInformation("All lines processed. Total unique words: {Count}", counts.Count);
            _logger.LogInformation("Map operation completed successfully.");
        }
    }
}
