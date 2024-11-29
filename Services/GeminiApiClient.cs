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
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

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
            Console.WriteLine("API Response:");
            Console.WriteLine(responseContent);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<GenerateContentResponse>(responseContent, options);

                    if (result != null && result.candidates != null && result.candidates.Count > 0)
                    {
                        var firstCandidate = result.candidates[0];
                        var firstPart = firstCandidate.content?.parts?[0];
                        var aiResponse = firstPart?.text ?? "No response";

                        return aiResponse;
                    }
                    else
                    {
                        Console.WriteLine("Deserialization returned null or no candidates found.");
                        return "No response";
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Deserialization error:");
                    Console.WriteLine(ex.Message);
                    return "An error occurred while processing the response.";
                }
            }
            else
            {
                Console.WriteLine("API call failed with status code: " + response.StatusCode);
                return "Failed to get a response from the AI.";
            }

        }
    }

    public class GenerateContentResponse
    {
        public List<Candidate> candidates { get; set; }
        public UsageMetadata usageMetadata { get; set; }
        public string modelVersion { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public double avgLogprobs { get; set; }
    }


    public class Content
    {
        public List<Part> parts { get; set; }
        public string role { get; set; }
    }
    public class Part
    {
        public string text { get; set; }
    }

    public class UsageMetadata
    {
        public int promptTokenCount { get; set; }
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
    }


}
