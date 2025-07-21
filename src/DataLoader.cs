using EspnFantasyLeagueHistoryDataLoader.Models.LeagueHistoryApiResponse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class DataLoader
{
    private readonly ILogger<DataLoader> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _espnClient;

    public DataLoader(ILogger<DataLoader> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _espnClient = httpClientFactory.CreateClient("EspnApiHttpClient");
    }

    public async Task LoadAllData()
    {
        _logger.LogInformation("Starting data loading process...");

        // Example: Load all years from the league history
        var years = await GetAllYears();
        _logger.LogInformation($"Loaded {years.Count} years of data.");

        // Additional data loading logic can be added here

        _logger.LogInformation("Data loading process completed.");
    }

    private async Task<List<int>> GetAllYears()
    {
        var response = await _espnClient.GetAsync($"/apis/v3/games/ffl/leagueHistory/{_configuration["LEAGUE_ID"]}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON array of years
            var data = System.Text.Json.JsonSerializer.Deserialize<List<LeagueHistoryApiResponse>>(content);
            return data.Select(d => d.seasonId).ToList();
        }
        else
        {
            _logger.LogError($"Failed to retrieve years: {response.StatusCode}");
        }

        return new List<int>();
    }
}