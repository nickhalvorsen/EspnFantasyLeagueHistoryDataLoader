namespace EspnFantasyLeagueHistoryDataLoader.Models;

public class Team
{
    public int EspnId { get; set; }
    public required string PrimaryOwnerId { get; set; }
    public required string Abbreviation { get; set; }
    public required string Name { get; set; }
    public required string LogoUrl { get; set; }
}
