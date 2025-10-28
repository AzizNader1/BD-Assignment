namespace BD_Assignment.Models.Responses
{
    public class CountryInfoResponse
    {
        public string Ip { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string Isp { get; set; } = string.Empty;
        public string? StateProv { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
