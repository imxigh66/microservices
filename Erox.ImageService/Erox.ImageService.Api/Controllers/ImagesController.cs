using Erox.ImageService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Erox.ImageService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/images")]
    public class ImagesController : ControllerBase
    {
        private readonly S3Service _s3Service;
        private readonly ILogger<ImagesController> _logger;
        public ImagesController(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var url = await _s3Service.UploadFileAsync(file);
            return Ok(new { url });
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteImage(string key)
        {
            try
            {
                
                await _s3Service.DeleteFileAsync(key);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, "Error deleting file");
            }
        }
    }
}
