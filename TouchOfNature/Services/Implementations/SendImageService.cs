using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using TouchOfNature.Services.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace TouchOfNature.Services.Implementations
{
    public class SendImageService : ISendImageService
    {
         private readonly HttpClient _client;

        public SendImageService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> SendImageAsync(IFormFile image)
        {

            using var content = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            content.Add(fileContent, "file", image.FileName);
            var response = await _client.PostAsync("https://rise-elevator-passage.ngrok-free.dev/predict", content);

            return await response.Content.ReadAsStringAsync();

        }
    }
}
