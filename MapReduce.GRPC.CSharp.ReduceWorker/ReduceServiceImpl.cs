using Grpc.Core;
using Mapreduce;

namespace MapReduce.GRPC.CSharp.ReduceWorker
{
    public class ReduceServiceImpl : ReduceService.ReduceServiceBase
    {
        private readonly ILogger<ReduceServiceImpl> _logger;
        public ReduceServiceImpl(ILogger<ReduceServiceImpl> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Reduce operation that aggregates word counts from multiple mappers
        // and returns the total count for each word.
        public override async Task Reduce(IAsyncStreamReader<WordCount> requestStream, IServerStreamWriter<WordCount> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("Starting Reduce operation...");
            var counts = new Dictionary<string, int>();
            // Read the stream of word counts
            await foreach (var wordCount in requestStream.ReadAllAsync())
            {
                counts[wordCount.Word] = counts.GetValueOrDefault(wordCount.Word) + wordCount.Count;
            }
            // Write back all (word,count) pairs
            foreach (var kv in counts)
            {
                await responseStream.WriteAsync(new WordCount { Word = kv.Key, Count = kv.Value });
            }
            _logger.LogInformation("All word counts processed. Total unique words: {Count}", counts.Count);
            _logger.LogInformation("Reduce operation completed successfully.");
        }
    }
  
}
