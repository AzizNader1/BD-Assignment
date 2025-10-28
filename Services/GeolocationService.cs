using BD_Assignment.Models.Responses;
using System.Text.Json;

namespace BD_Assignment.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeolocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeolocationApi:ApiKey"] ?? throw new InvalidOperationException("IPGeolocation API key not configured.");
            var baseUrl = configuration["GeolocationApi:BaseUrl"] ?? throw new InvalidOperationException("IPGeolocation API base URL not configured.");
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<CountryInfoResponse?> GetCountryInfoAsync(string ipAddress)
        {
            // Example for IPGeolocation.io: /ipgeo?apiKey=YOUR_KEY&ip=IP_ADDRESS
            var requestUri = $"ipgeo?apiKey={_apiKey}&ip={ipAddress}";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Handles potential case differences in JSON
                    };
                    var data = await JsonSerializer.DeserializeAsync<RootGeolocationResponse>(responseStream, options);

                    if (data != null)
                    {
                        // Map the response from IPGeolocation.io to your CountryInfoResponse model
                        return new CountryInfoResponse
                        {
                            Ip = data.Ip ?? ipAddress,
                            CountryCode = data.CountryCode2 ?? string.Empty, 
                            CountryName = data.CountryName ?? string.Empty, 
                            Isp = data.Isp ?? string.Empty,                 
                            StateProv = data.StateProv,
                            City = data.City,
                            Latitude = data.Latitude,
                            Longitude = data.Longitude
                        };
                    }
                }
                else
                {
                    // Log the specific error code if the API call failed
                    Console.WriteLine($"IPGeolocation API returned status code: {response.StatusCode} for IP {ipAddress}");
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON deserialization error for IP {ipAddress}: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching geolocation for IP {ipAddress}: {ex.Message}");
            }

            return null; // Return null if the call failed or deserialization failed
        }
    }

    // Example mapping class for IPGeolocation.io response structure
    public class RootGeolocationResponse
    {
        public string? Ip { get; set; }
        public string? CountryCode2 { get; set; } 
        public string? CountryName { get; set; }  
        public string? Isp { get; set; }         
        public string? StateProv { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}

