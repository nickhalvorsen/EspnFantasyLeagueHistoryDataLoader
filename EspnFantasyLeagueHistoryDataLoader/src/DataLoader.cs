using EspnFantasyLeagueHistoryDataLoader.Models;
using EspnFantasyLeagueHistoryDataLoader.Models.GetAllTeamsApiResponse;
using EspnFantasyLeagueHistoryDataLoader.Models.LeagueHistoryApiResponse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DataModels.Context;
using Microsoft.EntityFrameworkCore;
using Team = DataModels.Context.Team;

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
        var teams = new List<Team>();
        var yearlyStats = new List<TeamYearStat>();
        var weeklyStats = new List<TeamWeekStat>();

        foreach (var year in years)
        {
            var (thisYearsYearly, thisYearsWeekly) = await GetAllTeamYearStats(year);
            var espnTeamIdsToOmit = new List<string> { "4", "11" }; // Bin and Bri teams
            thisYearsYearly = thisYearsYearly.Where(yt => !espnTeamIdsToOmit.Contains(yt.Team.EspnId)).ToList();
            yearlyStats.AddRange(thisYearsYearly);

            thisYearsWeekly = thisYearsWeekly.Where(yt => !espnTeamIdsToOmit.Contains(yt.AwayTeamEspnId) && !espnTeamIdsToOmit.Contains(yt.HomeTeamEspnId)).ToList();
            weeklyStats.AddRange(thisYearsWeekly);
        }

        teams = yearlyStats.Select(yt => yt.Team).DistinctBy(t => t.EspnId).ToList();

        await SaveData(teams, yearlyStats, weeklyStats);

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

    private async Task<(List<TeamYearStat> YearStats, List<TeamWeekStat> WeekStats)> GetAllTeamYearStats(int year)
    {
        var response = await _espnClient.GetAsync($"/apis/v3/games/ffl/seasons/{year}/segments/0/leagues/{_configuration["LEAGUE_ID"]}?view=mTeam&view=mMatchupScore");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var leagueData = System.Text.Json.JsonSerializer.Deserialize<GetAllTeamsApiResponse>(content);
            var yearStats = leagueData.teams.Select(t => new DataModels.Context.TeamYearStat
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
            var weekStats = leagueData.schedule.Select(s => new TeamWeekStat
            {
                Year = leagueData.seasonId,
                WeekNumber = s.matchupPeriodId,
                AwayTeamEspnId = s.away.teamId.ToString(),
                HomeTeamEspnId = s.home.teamId.ToString(),
                AwayTeamScore = s.away.totalPoints,
                HomeTeamScore = s.home.totalPoints,
                Winner = s.winner
            }).ToList();

            return (yearStats, weekStats);
        }
        else
        {
            _logger.LogError($"Failed to retrieve teams for year {year}: {response.StatusCode}");
        }

        return (new List<TeamYearStat>(), new List<TeamWeekStat>());
    }

    private async Task SaveData(List<Team> teams, List<TeamYearStat> yearTeams, List<TeamWeekStat> teamWeekStats)
    {
        _logger.LogInformation("Saving data to the database...");

        var teamDbModels = teams.Select(t => new Team
        {
            EspnId = t.EspnId.ToString(),
            PrimaryOwnerId = t.PrimaryOwnerId,
            ManagerName = t.ManagerName,
            TeamYearStats = yearTeams
                .Where(yt => yt.Team.EspnId == t.EspnId)
                .Select(yt => new TeamYearStat
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
                _context.TeamWeekStats.RemoveRange(_context.TeamWeekStats);
                _context.DataLoaderInfos.RemoveRange(_context.DataLoaderInfos);
                await _context.SaveChangesAsync();
                _context.DataLoaderInfos.Add(new DataLoaderInfo
                {
                    LastSuccessfulLoad = DateTime.UtcNow,
                    Id = 1
                });
                _context.Teams.AddRange(teamDbModels);
                _context.TeamWeekStats.AddRange(teamWeekStats);
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
            }).ToListAsync(),
            TeamWeeks = await _context.TeamWeekStats.Select(tws => new TeamWeekStatDto
            {
                Year = tws.Year,
                Week = tws.WeekNumber,
                HomeTeamEspnId = tws.HomeTeamEspnId,
                AwayTeamEspnId = tws.AwayTeamEspnId,
                HomeTeamScore = tws.HomeTeamScore,
                AwayTeamScore = tws.AwayTeamScore,
                Winner = tws.Winner
            }).ToListAsync(),
            LastSuccessfulLoad = await _context.DataLoaderInfos
                .Select(dli => dli.LastSuccessfulLoad)
                .SingleAsync()
        };
    }
}