namespace TouchOfNature.Services.Interfaces;

public interface ISendImageService
{
    Task<string> SendImageAsync(IFormFile file);
}
