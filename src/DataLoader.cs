using EspnFantasyLeagueHistoryDataLoader.Models;
using EspnFantasyLeagueHistoryDataLoader.Models.GetAllTeamsApiResponse;
using EspnFantasyLeagueHistoryDataLoader.Models.LeagueHistoryApiResponse;
using EspnFantasyLeagueHistoryDataLoader.src.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Team = EspnFantasyLeagueHistoryDataLoader.Models.Team;

public class DataLoader
{
    private readonly ILogger<DataLoader> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _espnClient;
    private readonly XflLeagueHistoryContext _context;

    public DataLoader(ILogger<DataLoader> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, XflLeagueHistoryContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _espnClient = httpClientFactory.CreateClient("EspnApiHttpClient");
        _context = context;
    }

    public async Task LoadAllData()
    {
        _logger.LogInformation("Starting data loading process...");

        var years = await GetAllYears();

        var currentSeason = years.Max();
        var yearTeams = new List<TeamYearStats>();

        foreach (var year in years)
        {
            var thisYearsStats = await GetAllTeamYearStats(year);
            // filter out bin and bri
            thisYearsStats = thisYearsStats.Where(yt => yt.Team.PrimaryOwnerId == "{D1501A35-9B33-4FCC-BE17-2314248E1A9E}" || yt.Team.PrimaryOwnerId == "{0B7BB853-BE75-44B4-B41A-32EFCE2D42B1}").ToList();
            yearTeams.AddRange(thisYearsStats);
        }

        var teams = yearTeams.Select(yt => yt.Team).DistinctBy(t => t.EspnId).ToList();

        var test = _context.Teams.ToList();

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

    private async Task<List<TeamYearStats>> GetAllTeamYearStats(int year)
    {
        var response = await _espnClient.GetAsync($"/apis/v3/games/ffl/seasons/{year}/segments/0/leagues/{_configuration["LEAGUE_ID"]}?view=mTeam");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON response to get teams
            var leagueData = System.Text.Json.JsonSerializer.Deserialize<GetAllTeamsApiResponse>(content);
            return leagueData.teams.Select(t => new TeamYearStats
            {
                Team = new Team
                {
                    EspnId = t.id,
                    PrimaryOwnerId = t.primaryOwner,
                    Abbreviation = t.abbrev,
                    Name = t.name,
                    LogoUrl = t.logo
                },
                Year = leagueData.seasonId,
                Wins = t.record.overall.wins,
                Losses = t.record.overall.losses,
                Ties = t.record.overall.ties,
                PointsFor = t.record.overall.pointsFor,
                PointsAgainst = t.record.overall.pointsAgainst,
                DraftDayProjectedRank = t.draftDayProjectedRank,
                PlayoffSeed = t.playoffSeed,
                FinalRank = t.rankCalculatedFinal
            }).ToList();
        }
        else
        {
            _logger.LogError($"Failed to retrieve teams for year {year}: {response.StatusCode}");
        }

        return new List<TeamYearStats>();
    }
}