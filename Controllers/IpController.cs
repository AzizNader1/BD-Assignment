using BD_Assignment.Models.Responses;
using BD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IGeolocationService _geolocationService;
        private readonly InMemoryStorage _storage;

        public IpController(IGeolocationService geolocationService, InMemoryStorage storage)
        {
            _geolocationService = geolocationService;
            _storage = storage;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress = null)
        {
            // If IP is not provided, get the caller's IP
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                // Note: This might return "::1" for localhost. You might want to handle this differently for testing.
                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
                {
                    ipAddress = "8.8.8.8"; // Example fallback for local testing ONLY
                }
            }

            // Validate IP format (basic check)
            if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            {
                return BadRequest("Invalid IP address format.");
            }

            var countryInfo = await _geolocationService.GetCountryInfoAsync(ipAddress);

            if (countryInfo == null)
            {
                return NotFound(new { Message = $"Could not retrieve location information for IP: {ipAddress}" });
            }

            return Ok(countryInfo);
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlocked()
        {
            // 1. Fetch the caller's external IP address automatically using HttpContext
            var callerIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(callerIpAddress) || callerIpAddress == "::1" || callerIpAddress == "127.0.0.1")
            {
                callerIpAddress = "8.8.8.8"; // Fallback for local testing ONLY
            }

            // Validate IP format fetched from context (basic check)
            if (!System.Net.IPAddress.TryParse(callerIpAddress, out _))
            {
                // Log the attempt even if IP validation fails
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var logEntry = new BlockedAttemptLog
                {
                    IpAddress = callerIpAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = "INVALID_IP_FORMAT",
                    IsBlocked = true, // Consider invalid format as blocked attempt or log differently
                    UserAgent = userAgent
                };
                _storage.Logs.Add(logEntry);

                return BadRequest(new { Message = "Invalid IP address format detected." });
            }

            // 2. Fetch the country code using the third-party API using the fetched IP address.
            var countryInfo = await _geolocationService.GetCountryInfoAsync(callerIpAddress);

            if (countryInfo == null)
            {
                // Log the attempt even if lookup failed
                var userAgent2 = HttpContext.Request.Headers["User-Agent"].ToString();
                var logEntry2 = new BlockedAttemptLog
                {
                    IpAddress = callerIpAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = "UNKNOWN", // Or leave empty
                    IsBlocked = false, // Assume not blocked if lookup failed
                    UserAgent = userAgent2
                };
                _storage.Logs.Add(logEntry2);

                return StatusCode(500, new { Message = "Failed to determine country for your IP address." });
            }

            var countryCode = countryInfo.CountryCode.ToUpper();

            // 3. Check if the country is in the blocked list (permanent or temporal).
            var isBlocked = _storage.BlockedCountries.ContainsKey(countryCode) ||
                            (_storage.TemporalBlocks.TryGetValue(countryCode, out var tempBlock) && tempBlock.ExpiryTime > DateTime.UtcNow);

            // 4. Log the attempt.
            var userAgent3 = HttpContext.Request.Headers["User-Agent"].ToString();
            var logEntry3 = new BlockedAttemptLog
            {
                IpAddress = callerIpAddress,
                Timestamp = DateTime.UtcNow,
                CountryCode = countryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent3
            };
            _storage.Logs.Add(logEntry3);

            if (isBlocked)
            {
                return StatusCode(403, new { Message = "Access denied from your country." });
            }
            else
            {
                return Ok(new { Message = "Your country is not blocked.", CountryInfo = countryInfo });
            }
        }
    }
}
