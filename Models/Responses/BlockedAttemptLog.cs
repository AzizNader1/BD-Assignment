namespace BD_Assignment.Models.Responses
{
    public class BlockedAttemptLog
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }
}
