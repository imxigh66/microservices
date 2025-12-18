using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ICatalogClient _catalogClient;

        public TestController(ICatalogClient catalogClient)
        {
            _catalogClient = catalogClient;
        }

        [HttpGet("catalog")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckCatalog()
        {
            try
            {
                var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(1);
                return Ok(new { status = "connected", exists, name, price });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message
                });
            }
        }
    }
}
