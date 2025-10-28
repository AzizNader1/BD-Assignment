using BD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BD_Assignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly InMemoryStorage _storage;

        public LogsController(InMemoryStorage storage)
        {
            _storage = storage;
        }

        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            // Order by timestamp descending for most recent first
            var allLogs = _storage.Logs.OrderByDescending(l => l.Timestamp).AsEnumerable();

            var totalItems = allLogs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedLogs = allLogs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Items = pagedLogs,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
    }
}
