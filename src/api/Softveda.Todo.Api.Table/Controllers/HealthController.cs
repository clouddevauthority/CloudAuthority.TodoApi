using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softveda.Todo.Api.Table.Utils;

namespace Softveda.Todo.Api.Table.Controllers
{
	[Route("table/[controller]")]
	[ApiController]
	public class HealthController : ControllerBase
	{
		private readonly ILogger<HealthController> _logger;
		private readonly IConfiguration _configuration;
    private readonly string _storageConnectionString;
    private readonly string _tableName;

    public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
      _storageConnectionString = _configuration["StorageConnectionString"];
      _tableName = _configuration["TableName"];
    }

    /// <summary>
    /// Perform a Health check
    /// </summary>
    /// <returns>Status code</returns>
    /// <response code="200">If the health check is successfull</response>
    /// <response code="503">If the health check fails</response>
    [HttpGet(Name = "HealthCheck")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthCheck()
    {
      _logger.LogInformation("Checking Health of Application");

      var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

      var tableExists = await table.ExistsAsync();

      return tableExists ? new OkResult() : new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
    }
  }
}
