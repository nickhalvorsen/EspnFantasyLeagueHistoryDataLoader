
using DataModels.Context;

namespace EspnFantasyLeagueHistoryDataLoader.Models;

public class FetchDataDto
{
    public List<TeamDto> Teams { get; set; }
    public List<TeamYearStatDto> TeamYears { get; set; }
    public DateTime LastSuccessfulLoad { get; set; }
}

public class TeamDto
{
    public string EspnId { get; set; }
    public string PrimaryOwnerId { get; set; }
    public string ManagerName { get; set; }
}

public class TeamYearStatDto
{
    public int Year { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public decimal PointsFor { get; set; }
    public decimal PointsAgainst { get; set; }
    public int PlayoffSeed { get; set; }
    public int FinalRank { get; set; }
    public string TeamEspnId { get; set; }
}
