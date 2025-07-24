namespace EspnFantasyLeagueHistoryDataLoader.Models;

public class Team
{
    public int EspnId { get; set; }
    public required string PrimaryOwnerId { get; set; }
    public required string ManagerName { get; set; }
}
