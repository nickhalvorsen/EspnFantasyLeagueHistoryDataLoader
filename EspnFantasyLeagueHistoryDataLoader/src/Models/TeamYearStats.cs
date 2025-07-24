namespace EspnFantasyLeagueHistoryDataLoader.Models;

public class TeamYearStats
{
    public required Team Team { get; set; }
    public int Year { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public decimal PointsFor { get; set; }
    public decimal PointsAgainst { get; set; }
    public int PlayoffSeed { get; set; }
    public int FinalRank { get; set; }
}
