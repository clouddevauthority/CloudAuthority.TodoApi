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

    public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
    }

		/// <summary>
		/// Perform a Health check
		/// </summary>
		/// <returns>Status code</returns>
		/// <response code="200">If the health check is successfull</response>
		[HttpGet(Name = "HealthCheck")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<string> GetHealthCheck()
		{
			_logger.LogInformation("Checking Health of Application");

			return Ok("HealthCheck OK");
		}
	}
}
