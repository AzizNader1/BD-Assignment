namespace BD_Assignment.Services
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly ILogger<TemporalBlockCleanupService> _logger;
        private readonly InMemoryStorage _storage;

        public TemporalBlockCleanupService(ILogger<TemporalBlockCleanupService> logger, InMemoryStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5)); // Run every 5 minutes

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var expiredKeys = _storage.TemporalBlocks
                        .Where(kvp => kvp.Value.ExpiryTime <= now)
                        .Select(kvp => kvp.Key)
                        .ToList(); // ToList to avoid modifying collection during enumeration

                    foreach (var countryCode in expiredKeys)
                    {
                        if (_storage.TemporalBlocks.TryRemove(countryCode, out _))
                        {
                            _logger.LogInformation($"Temporal block for country {countryCode} has expired and been removed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired temporal blocks.");
                }
            }
        }
    }
}
