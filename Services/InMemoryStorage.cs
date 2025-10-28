using BD_Assignment.Models.Internal;
using BD_Assignment.Models.Responses;
using System.Collections.Concurrent;

namespace BD_Assignment.Services
{
    public class InMemoryStorage
    { // Store permanently blocked countries (Key: CountryCode)
        public ConcurrentDictionary<string, string> BlockedCountries { get; } = new();

        // Store temporarily blocked countries with expiry time (Key: CountryCode)
        public ConcurrentDictionary<string, TemporalBlock> TemporalBlocks { get; } = new();

        // Store logs of check attempts
        public ConcurrentBag<BlockedAttemptLog> Logs { get; } = new();
    }
}
