using System.Text.Json;
using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BoltBrain.Services
{
    public class GeminiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/";

        public GeminiApiClient(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiAPI:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key is not configured.");
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var requestUri = $"models/gemini-1.5-flash-latest:generateContent?key={_apiKey}";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GenerateContentResponse>(responseContent);
                return result?.contents?[0]?.parts?[0]?.text ?? "No response";
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}, Details: {responseContent}");
            }

        }
    }

    public class GenerateContentResponse
    {
        public Content[] contents { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}
