using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using TouchOfNature.Services.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;

namespace TouchOfNature.Services.Implementations
{
    public class SendImageService : ISendImageService
    {
         private readonly HttpClient _client;
         private readonly IConfiguration _configuration;

        public SendImageService(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task<string> SendImageAsync(IFormFile image)
        {

            using var content = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            content.Add(fileContent, "file", image.FileName);
            
            var predictUrl = _configuration["PredictUrl"];
            var response = await _client.PostAsync(predictUrl, content);

            return await response.Content.ReadAsStringAsync();

        }
    }
}
