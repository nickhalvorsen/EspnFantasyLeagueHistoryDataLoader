namespace EspnFantasyLeagueHistoryDataLoader.Models;

public class TeamYearStats
{
    public required Team Team { get; set; }
    public int Year { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public double PointsFor { get; set; }
    public double PointsAgainst { get; set; }
    public int DraftDayProjectedRank { get; set; }
    public int PlayoffSeed { get; set; }
    public int FinalRank { get; set; }
}
