using System.Text.Json;
using DashboardApp.Models;

namespace DashboardApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<DashboardRecord>> GetRecordsAsync()
        {
            var url = _configuration["ApiSettings:Url"];

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException(
                    "ApiSettings:Url is not set in appsettings.json. Add your real API URL there.");
            }

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not reach API at {Url}", url);
                throw new ApplicationException("Could not connect to the data API. Please try again shortly.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API returned {StatusCode} from {Url}", response.StatusCode, url);
                throw new ApplicationException($"API returned an error: {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();

            try
            {
                // API shape confirmed: { "data": [ {...}, {...} ] }
                var result = JsonSerializer.Deserialize<DashboardApiResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result?.Data ?? new List<DashboardRecord>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse API response from {Url}. Raw error path: {Path}", url, ex.Path);
                throw new ApplicationException(
                    $"Received an unexpected response format from the API (near {ex.Path ?? "start of document"}).", ex);
            }
        }
    }
}