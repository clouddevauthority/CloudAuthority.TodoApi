using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Softveda.Todo.Api.InMemory.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class HealthController : ControllerBase
	{
		private readonly ILogger<HealthController> _logger;
		private readonly IConfiguration _configuration;
		private readonly string HealthCheckKey;

		public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			HealthCheckKey = _configuration.GetSection("HealthCheckKey").Value;
		}

		/// <summary>
		/// Perform a Health check
		/// </summary>
		/// <param name="key">A key that must be provided to this endpoint</param>
		/// <returns>Status code</returns>
		/// <response code="200">If the health check is successfull</response>
		/// <response code="403">If the key is incorrect</response>
		[HttpGet("{key}", Name = "HealthCheck")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public ActionResult<string> GetHealthCheck([FromRoute] string key)
		{
			_logger.LogInformation("Checking Health of Application");
			if (string.CompareOrdinal(key, HealthCheckKey) != 0)
			{
				return Forbid();
			}

			return Ok("HealthCheck OK");
		}
	}
}
