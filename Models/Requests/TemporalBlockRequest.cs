namespace BD_Assignment.Models.Requests
{
    public class TemporalBlockRequest
    {
        public string CountryCode { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } // Between 1 and 1440 (24 hours)
    }
}
