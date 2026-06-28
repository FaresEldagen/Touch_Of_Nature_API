using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ModelController : ControllerBase
{
    private readonly ISendImageService _sendImageService;

    public ModelController(ISendImageService sendImageService)
    {
        _sendImageService = sendImageService;
    }

    [HttpPost("predict-leaf-disease")]
    public async Task<IActionResult> Integrate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Only image files are allowed");

        var allowedContentTypes = new[] { "image/jpeg", "image/png" };

        if (!allowedContentTypes.Contains(file.ContentType))
            return BadRequest("Invalid image content type");

        var result = await _sendImageService.SendImageAsync(file);

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(result);

        var prettyJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Ok(prettyJson);
    }
}
