using EspnFantasyLeagueHistoryDataLoader.Models;
using EspnFantasyLeagueHistoryDataLoader.Models.GetAllTeamsApiResponse;
using EspnFantasyLeagueHistoryDataLoader.Models.LeagueHistoryApiResponse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeamDbModel = DataModels.Context.Team;
using DataModels.Context;
using Microsoft.EntityFrameworkCore;

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
        var yearTeams = new List<TeamYearStat>();

        foreach (var year in years)
        {
            var thisYearsStats = await GetAllTeamYearStats(year);
            // filter out bin and bri
            thisYearsStats = thisYearsStats.Where(yt => yt.Team.PrimaryOwnerId != "{D1501A35-9B33-4FCC-BE17-2314248E1A9E}" && yt.Team.PrimaryOwnerId != "{0B7BB853-BE75-44B4-B41A-32EFCE2D42B1}").ToList();
            yearTeams.AddRange(thisYearsStats);
        }

        var teams = yearTeams.Select(yt => yt.Team).DistinctBy(t => t.EspnId).ToList();

        await SaveData(teams, yearTeams);

        var test = _context.Teams.ToList();

        _logger.LogInformation("Data loading process completed.");
    }

    private async Task<List<int>> GetAllYears()
    {
        var response = await _espnClient.GetAsync($"/apis/v3/games/ffl/leagueHistory/{_configuration["LEAGUE_ID"]}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<List<LeagueHistoryApiResponse>>(content);
            return data.Select(d => d.seasonId).ToList();
        }
        else
        {
            _logger.LogError($"Failed to retrieve years: {response.StatusCode}");
        }

        return new List<int>();
    }

    private async Task<List<TeamYearStat>> GetAllTeamYearStats(int year)
    {
        var response = await _espnClient.GetAsync($"/apis/v3/games/ffl/seasons/{year}/segments/0/leagues/{_configuration["LEAGUE_ID"]}?view=mTeam");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var leagueData = System.Text.Json.JsonSerializer.Deserialize<GetAllTeamsApiResponse>(content);
            return leagueData.teams.Select(t => new DataModels.Context.TeamYearStat
            {
                Team = new DataModels.Context.Team
                {
                    EspnId = t.id.ToString(),
                    PrimaryOwnerId = t.primaryOwner,
                    ManagerName = leagueData.members.First(m => m.id == t.primaryOwner).firstName.Substring(0, 1).ToUpper() + leagueData.members.First(m => m.id == t.primaryOwner).firstName.Substring(1)
                },
                Year = leagueData.seasonId,
                Wins = t.record.overall.wins,
                Losses = t.record.overall.losses,
                Ties = t.record.overall.ties,
                PointsFor = t.record.overall.pointsFor,
                PointsAgainst = t.record.overall.pointsAgainst,
                PlayoffSeed = t.playoffSeed,
                FinalRank = t.rankCalculatedFinal
            }).ToList();
        }
        else
        {
            _logger.LogError($"Failed to retrieve teams for year {year}: {response.StatusCode}");
        }

        return new List<TeamYearStat>();
    }

    private async Task SaveData(List<DataModels.Context.Team> teams, List<TeamYearStat> yearTeams)
    {
        _logger.LogInformation("Saving data to the database...");

        var teamDbModels = teams.Select(t => new TeamDbModel
        {
            EspnId = t.EspnId.ToString(),
            PrimaryOwnerId = t.PrimaryOwnerId,
            ManagerName = t.ManagerName,
            TeamYearStats = yearTeams
                .Where(yt => yt.Team.EspnId == t.EspnId)
                .Select(yt => new DataModels.Context.TeamYearStat
                {
                    Year = yt.Year,
                    Wins = yt.Wins,
                    Losses = yt.Losses,
                    Ties = yt.Ties,
                    PointsFor = yt.PointsFor,
                    PointsAgainst = yt.PointsAgainst,
                    PlayoffSeed = yt.PlayoffSeed,
                    FinalRank = yt.FinalRank
                }).ToList()
        }).ToList();

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                _context.TeamYearStats.RemoveRange(_context.TeamYearStats);
                _context.Teams.RemoveRange(_context.Teams);
                await _context.SaveChangesAsync();
                _context.Teams.AddRange(teamDbModels);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        _logger.LogInformation("Data saved successfully.");
    }

    public async Task<FetchDataDto> GetAllData()
    {
        return new FetchDataDto
        {
            Teams = await _context.Teams.Select(t => new TeamDto
            {
                EspnId = t.EspnId,
                PrimaryOwnerId = t.PrimaryOwnerId,
                ManagerName = t.ManagerName
            }).ToListAsync(),
            TeamYears = await _context.TeamYearStats.Select(tys => new TeamYearStatDto
            {
                Year = tys.Year,
                Wins = tys.Wins,
                Losses = tys.Losses,
                Ties = tys.Ties,
                PointsFor = tys.PointsFor,
                PointsAgainst = tys.PointsAgainst,
                PlayoffSeed = tys.PlayoffSeed,
                FinalRank = tys.FinalRank,
                TeamEspnId = tys.Team.EspnId
            }).ToListAsync()
        };
    }
}