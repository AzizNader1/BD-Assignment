using BD_Assignment.Models.Internal;
using BD_Assignment.Models.Requests;
using BD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly InMemoryStorage _storage;

        public CountriesController(InMemoryStorage storage)
        {
            _storage = storage;
        }

        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode))
            {
                return BadRequest("Country code is required.");
            }

            var countryCode = request.CountryCode.ToUpper();

            // Validate basic country code format (2 letters)
            if (countryCode.Length != 2 || !countryCode.All(char.IsLetter))
            {
                return BadRequest("Invalid country code format. Use ISO 3166-1 alpha-2 (e.g., US, EG).");
            }

            // Check if already permanently blocked
            if (_storage.BlockedCountries.ContainsKey(countryCode))
            {
                return Conflict("Country is already permanently blocked.");
            }

            // Check if temporarily blocked
            if (_storage.TemporalBlocks.ContainsKey(countryCode))
            {
                return Conflict("Country is already temporarily blocked.");
            }

            // Add to permanently blocked list
            _storage.BlockedCountries.TryAdd(countryCode, countryCode);

            return Ok(new { Message = $"Country {countryCode} has been permanently blocked." });
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return BadRequest("Country code is required.");
            }

            countryCode = countryCode.ToUpper();

            // Validate basic country code format (2 letters)
            if (countryCode.Length != 2 || !countryCode.All(char.IsLetter))
            {
                return BadRequest("Invalid country code format. Use ISO 3166-1 alpha-2 (e.g., US, EG).");
            }

            if (_storage.BlockedCountries.TryRemove(countryCode, out _))
            {
                return Ok(new { Message = $"Country {countryCode} has been unblocked." });
            }
            else
            {
                return NotFound(new { Message = $"Country {countryCode} is not currently permanently blocked." });
            }
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var allBlockedCountries = _storage.BlockedCountries.Keys.AsEnumerable();

            // Apply search/filter if provided (filter by country code)
            if (!string.IsNullOrWhiteSpace(search))
            {
                allBlockedCountries = allBlockedCountries.Where(c => c.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = allBlockedCountries.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedCountries = allBlockedCountries
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Items = pagedCountries,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }

        [HttpPost("temporal-block")]
        public IActionResult TemporalBlockCountry([FromBody] TemporalBlockRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CountryCode))
            {
                return BadRequest("Country code is required.");
            }

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440) // 1440 minutes = 24 hours
            {
                return BadRequest("Duration must be between 1 and 1440 minutes (24 hours).");
            }

            var countryCode = request.CountryCode.ToUpper();

            // Validate basic country code format (2 letters)
            if (countryCode.Length != 2 || !countryCode.All(char.IsLetter))
            {
                return BadRequest("Invalid country code format. Use ISO 3166-1 alpha-2 (e.g., US, EG).");
            }

            // Check if already permanently blocked
            if (_storage.BlockedCountries.ContainsKey(countryCode))
            {
                return Conflict("Country is already permanently blocked.");
            }

            // Check if already temporarily blocked
            if (_storage.TemporalBlocks.ContainsKey(countryCode))
            {
                return Conflict("Country is already temporarily blocked.");
            }

            var expiryTime = DateTime.UtcNow.AddMinutes(request.DurationMinutes);
            var temporalBlock = new TemporalBlock { CountryCode = countryCode, ExpiryTime = expiryTime };

            if (_storage.TemporalBlocks.TryAdd(countryCode, temporalBlock))
            {
                return Ok(new { Message = $"Country {countryCode} has been temporarily blocked for {request.DurationMinutes} minutes." });
            }
            else
            {
                return Conflict("Failed to add temporal block. Please try again.");
            }
        }
    }
}
