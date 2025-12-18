using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
	[ApiController]
	[Route("api")]
	public class GatewayInfoController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public GatewayInfoController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Получить информацию о API Gateway
		/// </summary>
		[HttpGet("info")]
		[ProducesResponseType(200)]
		public IActionResult GetInfo()
		{
			return Ok(new
			{
				service = "API Gateway",
				version = "1.0.0",
				status = "healthy",
				timestamp = DateTime.UtcNow,
				routes = new
				{
					catalog = "/catalog/api/*",
					users = "/users/api/*",
					payments = "/payments/api/*",
					identity = "/identity/api/*",
					images = "/images/api/*"
				}
			});
		}

		/// <summary>
		/// Health check endpoint
		/// </summary>
		[HttpGet("health")]
		[ProducesResponseType(200)]
		public IActionResult HealthCheck()
		{
			return Ok(new
			{
				status = "healthy",
				timestamp = DateTime.UtcNow
			});
		}
	}
}